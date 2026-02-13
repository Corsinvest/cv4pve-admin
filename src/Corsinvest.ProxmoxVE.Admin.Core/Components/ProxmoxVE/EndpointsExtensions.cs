/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Net;
using System.Net.WebSockets;
using System.Web;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE;

internal static class EndpointsExtensions
{
    private record PBSBackupRestorFileInfo(string ClusterName,
                                           string Node,
                                           string Storage,
                                           string Volume,
                                           string FilePath,
                                           string Type,
                                           string FileName,
                                           bool Tar);

    public static string GetUrlDowloadFileBackup(IFusionCache fusionCache,
                                                 string clusterName,
                                                 string node,
                                                 string storage,
                                                 string volume,
                                                 string filePath,
                                                 string type,
                                                 string fileName,
                                                 bool tar)
    {
        var key = Guid.NewGuid().ToString();
        fusionCache.Set(key,
                        new PBSBackupRestorFileInfo(clusterName,
                                                          node,
                                                          storage,
                                                          volume,
                                                          filePath,
                                                          type,
                                                          fileName,
                                                          tar),
                        TimeSpan.FromSeconds(10));

        return $"/backup-download-file/{key}";
    }

    public static string GetWebConsoleRouteBase(string clusterName) => $"webconsole/{clusterName}";

    public static IEndpointRouteBuilder MapPveEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(string.Empty)
                             .RequireAuthorization();
        group.MapWebConsole();
        group.MapBackup();
        group.MapSpice();

        return endpoints;
    }

    private static IEndpointRouteBuilder MapBackup(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/backup-download-file/{key}",
                         async (string key,
                                IFusionCache fusionCache,
                                IAdminService adminService,
                                IPermissionService permissionService) =>
        {
            var info = fusionCache.TryGet<PBSBackupRestorFileInfo>(key).Value;

            if (info != null)
            {
                fusionCache.Remove(key);

                if (info != null)
                {
                    if (!long.TryParse(info.Volume.Split("/")[2], out var vmId)) { return Results.BadRequest(); }
                    if (!await permissionService.HasVmAsync(info.ClusterName, ClusterPermissions.Vm.BackupRestoreFile, vmId)) { return Results.Unauthorized(); }

                    var client = await adminService[info.ClusterName].GetPveClientAsync();
                    var url = BackupHelper.GetDownloadFileUrl(client.Host,
                                                              client.Port,
                                                              info.Node,
                                                              info.Storage,
                                                              info.Volume,
                                                              info.FilePath);

                    var httpClient = client.GetHttpClient();
                    //var stream = await httpClient.GetStreamAsync(url);

                    var request = client.CreateHttpRequestMessage(HttpMethod.Get, url);
                    var response = await httpClient.SendAsync(request);
                    var stream = await response.Content.ReadAsStreamAsync();

                    var fileName = info.FileName;
                    if (info.Type == "d")
                    {
                        if (info.Tar)
                        {
                            url += "?tar=1";
                            fileName += ".tar.zst";
                        }
                        else
                        {
                            fileName += ".zip";
                        }
                    }

                    return Results.File(stream, "application/octet-stream", fileName);
                }
            }

            return Results.BadRequest();
        });

        return endpoints;
    }

    private static IEndpointRouteBuilder MapWebConsole(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/webconsole/{clusterName}");

        group.MapGet("/", async (string clusterName,
                                        [FromQuery] long vmId,
                                        [FromQuery] string node,
                                        [FromQuery] string vmName,
                                        [FromQuery] string console,
                                        [FromQuery] string? noVnc,
                                        [FromQuery] string? xTermJs,
                                        IAdminService adminService,
                                        IPermissionService permissionService)
            => await GetConsoleAsync(clusterName,
                                     vmId,
                                     node,
                                     vmName,
                                     console,
                                     noVnc == "1",
                                     xTermJs == "1",
                                     adminService,
                                     permissionService));

        static async Task<IResult> GetConsoleAsync(string clusterName,
                                                   long vmId,
                                                   string node,
                                                   string vmName,
                                                   string console,
                                                   bool noVnc,
                                                   bool xTermJs,
                                                   IAdminService adminService,
                                                   IPermissionService permissionService)
        {
            #region check permissions
            var valid = vmId == 0
                        ? await permissionService.HasNodeAsync(clusterName, ClusterPermissions.Node.Console, node)
                        : await permissionService.HasVmAsync(clusterName, ClusterPermissions.Vm.Console, vmId);

            if (!valid) { return Results.Unauthorized(); }
            #endregion

            var client = await adminService[clusterName].GetPveClientAsync();
            var response = await VmHelper.GetConsoleNoVncAsync(client,
                                                               node,
                                                               vmId,
                                                               vmName,
                                                               console,
                                                               noVnc,
                                                               xTermJs);

            response.Headers.Add("CSRFPreventionToken", client.CSRFPreventionToken);
            var ret = await response.Content.ReadAsStringAsync();

            var routeBase = GetWebConsoleRouteBase(clusterName);
            if (noVnc) { ret = ret.Replace("/novnc/", $"/{routeBase}/novnc/"); }
            if (xTermJs) { ret = ret.Replace("/xtermjs/", $"/{routeBase}/xtermjs/"); }
            if (console == "shell")
            {
                //                ret = ret.Replace("=\"/", $"=\"/{routeBase}/");
                //="/
                //ret = ret.Replace("/pve2/", $"/{routeBase}/pve2/");
            }
            return Results.Content(ret, response.Content.Headers.ContentType!.MediaType);
        }

        group.MapGet("/xtermjs/{*resource}",
                     async (string clusterName, string resource, IAdminService adminService) =>
        {
            return resource switch
            {
                "main.js" or "util.js" => await FixJsAsync(clusterName, $"/xtermjs/{resource}", adminService),
                _ => await GetResourceFromPveAsync(clusterName, $"xtermjs/{resource}", adminService)
            };
        });

        group.MapGet("/novnc/{*resource}",
                     async (string clusterName, string resource, IAdminService adminService) =>
        {
            return resource switch
            {
                "app.js" => await FixJsAsync(clusterName, $"/novnc/{resource}", adminService),
                _ => await GetResourceFromPveAsync(clusterName, $"novnc/{resource}", adminService)
            };
        });

        static async Task<IResult> FixJsAsync(string clusterName, string resource, IAdminService adminService)
        {
            var client = await adminService[clusterName].GetPveClientAsync();
            var httpClient = client.GetHttpClient();
            var request = client.CreateHttpRequestMessage(HttpMethod.Get, $"{client.BaseAddress}{resource}");
            using var responseMessage = await httpClient.SendAsync(request);
            responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync();

            var routeBase = GetWebConsoleRouteBase(clusterName);
            response = response.Replace("/novnc/", $"/{routeBase}/novnc/");
            response = response.Replace("/xtermjs/", $"/{routeBase}/xtermjs/");
            response = response.Replace("api2/json", $"{routeBase}/api2/json");

            return Results.Content(response, "application/javascript");
        }

        static async Task<IResult> GetResourceFromPveAsync(string clusterName, string resource, IAdminService adminService)
        {
            var client = await adminService[clusterName].GetPveClientAsync();
            var httpClient = client.GetHttpClient();
            var request = client.CreateHttpRequestMessage(HttpMethod.Get, $"{client.BaseAddress}/{resource}");
            var response = await httpClient.SendAsync(request);

            var stream = new MemoryStream();
            await response.Content.CopyToAsync(stream);
            stream.Position = 0;
            return Results.Stream(stream, response.Content.Headers.ContentType!.MediaType);
        }

        static async Task TransferDataAsync(WebSocket source, WebSocket destination)
        {
            var buffer = new byte[4096];
            while (source.State == WebSocketState.Open && destination.State == WebSocketState.Open)
            {
                var result = await source.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await destination.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }

                await destination.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count),
                                            result.MessageType,
                                            result.EndOfMessage,
                                            CancellationToken.None);
            }

            try
            {
                await source.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            catch { }
            try
            {
                await destination.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            catch { }
        }

        group.MapPost("/api2/json/nodes/{node}/termproxy",
                      async (string clusterName,
                             string node,
                             IAdminService adminService,
                             IPermissionService permissionService) =>
        {
            //check permissions
            if (!await permissionService.HasNodeAsync(clusterName, ClusterPermissions.Node.Console, node)) { return Results.Unauthorized(); }

            var client = await adminService[clusterName].GetPveClientAsync();
            var result = await client.Nodes[node].Termproxy.Termproxy();
            return Results.Json(result?.Response);
        });

        group.MapPost("/api2/json/nodes/{node}/{type}/{vmId}/termproxy",
                      async (string clusterName,
                             string node,
                             string type,
                             long vmId,
                             IAdminService adminService,
                             IPermissionService permissionService) =>
        {
            //check permissions
            if (!await permissionService.HasVmAsync(clusterName, ClusterPermissions.Vm.Console, vmId)) { return Results.Unauthorized(); }

            var client = await adminService[clusterName].GetPveClientAsync();
            var result = GetVmType(type) switch
            {
                VmType.Qemu => await client.Nodes[node].Qemu[vmId].Termproxy.Termproxy(),
                VmType.Lxc => await client.Nodes[node].Lxc[vmId].Termproxy.Termproxy(),
                _ => throw new InvalidEnumArgumentException()
            };

            return Results.Json(result?.Response);
        });

        group.MapPost("/api2/json/nodes/{node}/vncshell",
                      async (string clusterName,
                             string node,
                             HttpRequest request,
                             IAdminService adminService,
                             IPermissionService permissionService) =>
        {
            //check permissions
            if (!await permissionService.HasNodeAsync(clusterName, ClusterPermissions.Node.Console, node)) { return Results.Unauthorized(); }

            var from = await request.ReadFormAsync();
            var websocket = from["websocket"][0] == "1";

            var client = await adminService[clusterName].GetPveClientAsync();
            var result = await client.Nodes[node].Vncshell.Vncshell(websocket: websocket);
            return Results.Json(result?.Response);
        });

        group.MapPost("/api2/json/nodes/{node}/{type}/{vmId}/vncproxy",
                      async (string clusterName,
                             string node,
                             string type,
                             long vmId,
                             HttpRequest request,
                             IAdminService adminService,
                             IPermissionService permissionService) =>
        {
            //check permissions
            if (!await permissionService.HasVmAsync(clusterName, ClusterPermissions.Vm.Console, vmId)) { return Results.Unauthorized(); }

            var from = await request.ReadFormAsync();
            var websocket = from["websocket"][0] == "1";

            var client = await adminService[clusterName].GetPveClientAsync();
            var result = GetVmType(type) switch
            {
                VmType.Qemu => await client.Nodes[node].Qemu[vmId].Vncproxy.Vncproxy(websocket: websocket),
                VmType.Lxc => await client.Nodes[node].Lxc[vmId].Vncproxy.Vncproxy(websocket: websocket),
                _ => throw new InvalidEnumArgumentException()
            };

            return Results.Json(result?.Response);
        });

        group.Map("/api2/json/nodes/{node}/vncwebsocket",
                  async (string clusterName,
                         string node,
                         [FromQuery] int port,
                         [FromQuery] string vncticket,
                         HttpContext httpContext,
                         IAdminService adminService,
                         IPermissionService permissionService)
            => await CreateVncWebSocketAsync(clusterName,
                                             node,
                                             string.Empty,
                                             0,
                                             port,
                                             vncticket,
                                             httpContext,
                                             adminService,
                                             permissionService));

        group.Map("/api2/json/nodes/{node}/{type}/{vmId}/vncwebsocket",
                  async (string clusterName,
                         string node,
                         string type,
                         long vmId,
                         [FromQuery] int port,
                         [FromQuery] string vncticket,
                         HttpContext httpContext,
                         IAdminService adminService,
                         IPermissionService permissionService)
            => await CreateVncWebSocketAsync(clusterName,
                                             node,
                                             type,
                                             vmId,
                                             port,
                                             vncticket,
                                             httpContext,
                                             adminService,
                                             permissionService));

        static async Task<IResult> CreateVncWebSocketAsync(string clusterName,
                                                           string node,
                                                           string type,
                                                           long vmId,
                                                           int port,
                                                           string vncticket,
                                                           HttpContext httpContext,
                                                           IAdminService adminService,
                                                           IPermissionService permissionService)
        {
            #region check permissions
            var valid = vmId == 0
                        ? await permissionService.HasNodeAsync(clusterName, ClusterPermissions.Node.Console, node)
                        : await permissionService.HasVmAsync(clusterName, ClusterPermissions.Vm.Console, vmId);

            if (!valid) { return Results.Unauthorized(); }
            #endregion

            //acquisisco socket
            using var ws = await httpContext.WebSockets.AcceptWebSocketAsync(new WebSocketAcceptContext
            {
                SubProtocol = "binary"
                //DangerousEnableCompression = true,
            });

            var clusterClient = adminService[clusterName];

            var ticket = HttpUtility.UrlEncode(vncticket);
            var client = await clusterClient.GetPveClientAsync();

            var uri = vmId == 0 && string.IsNullOrEmpty(type)
                        ? new Uri(NoVncHelper.GetWebsocketUrl(client.Host, client.Port, node, port, ticket))
                        : new Uri(NoVncHelper.GetWebsocketUrl(client.Host, client.Port, node, type, vmId, port, ticket));

            using var wsPve = new ClientWebSocket();
            wsPve.Options.AddSubProtocol("binary");
            wsPve.Options.SetRequestHeader("CSRFPreventionToken", client.CSRFPreventionToken);
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Cookie("PVEAuthCookie", client.PVEAuthCookie, "/", client.Host)
            {
                //Expires = DateTime.Now.AddDays(1),
                Secure = true
            });

            wsPve.Options.Cookies = cookieContainer;

            if (!clusterClient.Settings.ValidateCertificate)
            {
                wsPve.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
            }

            wsPve.Options.KeepAliveInterval = TimeSpan.FromSeconds(120);

            await wsPve.ConnectAsync(uri, CancellationToken.None);

            var t1 = TransferDataAsync(wsPve, ws);
            var t2 = TransferDataAsync(ws, wsPve);

            await Task.WhenAny(t1, t2);

            return Results.Empty;
        }

        static VmType GetVmType(string type) => Enum.Parse<VmType>(type, true);

        group.MapGet("/api2/json/nodes/{node}/{type}/{vmId}/status/{status}",
                     async (string clusterName,
                            string node,
                            string type,
                            long vmId,
                            string status,
                            IAdminService adminService,
                            IPermissionService permissionService) =>
        {
            //check permissions
            if (!await permissionService.HasVmAsync(clusterName, ClusterPermissions.Vm.Console, vmId)) { return Results.Unauthorized(); }

            _ = status; // dummy status current

            var client = await adminService[clusterName].GetPveClientAsync();
            var result = GetVmType(type) switch
            {
                VmType.Qemu => await client.Nodes[node].Qemu[vmId].Status.Current.VmStatus(),
                VmType.Lxc => await client.Nodes[node].Lxc[vmId].Status.Current.VmStatus(),
                _ => throw new InvalidEnumArgumentException()
            };

            return Results.Json(result?.Response);
        });

        group.MapPost("/api2/json/nodes/{node}/{type}/{vmId}/status/{status}",
                      async (string clusterName,
                             string node,
                             string type,
                             long vmId,
                             string status,
                             IAdminService adminService,
                             IPermissionService permissionService) =>
        {
            //check permissions
            if (!await permissionService.HasVmAsync(clusterName, ClusterPermissions.Vm.PowerManagement, vmId)) { return Results.Unauthorized(); }

            var result = await VmHelper.ChangeStatusVmAsync(await adminService[clusterName].GetPveClientAsync(),
                                                            node,
                                                            GetVmType(type),
                                                            vmId,
                                                            Enum.Parse<VmStatus>(status, true));

            return Results.Json(result?.Response);
        });

        return endpoints;
    }

    private static IEndpointRouteBuilder MapSpice(this IEndpointRouteBuilder endpoints) =>
        //endpoints.Map("/spice/{clusterName}", async (HttpContext context, string clusterName) =>
        //{
        //    var aa = 1;

        //    //var hostId = context.Request.Query["hostId"].ToString();
        //    //if (!proxmoxHosts.TryGetValue(hostId, out var proxmoxHost))
        //    //{
        //    //    context.Response.StatusCode = 400;
        //    //    await context.Response.WriteAsync("Host ID non valido.");
        //    //    return;
        //    //}

        //    //try
        //    //{
        //    //    var client = await context.Features.Get<TcpClient>();
        //    //    if (client is null)
        //    //    {
        //    //        context.Response.StatusCode = 500;
        //    //        await context.Response.WriteAsync("Errore nel recupero del client TCP.");
        //    //        return;
        //    //    }

        //    //    var tunnelService = new SpiceTunnelService();
        //    //    await tunnelService.HandleSpiceRequest(proxmoxHost, client);
        //    //    context.Response.StatusCode = 200;
        //    //    await context.Response.WriteAsync("Connessione inoltrata.");
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    context.Response.StatusCode = 500;
        //    //    await context.Response.WriteAsync($"Errore durante la connessione: {ex.Message}");
        //    //}
        //});
        endpoints;
    //static async Task HandleSpiceRequest(string proxmoxHost, TcpClient client)//{//    int spicePort = 3128; // Porta SPICE su Proxmox//    using var remoteClient = new TcpClient();//    //try//    //{//    await remoteClient.ConnectAsync(proxmoxHost, spicePort);//    var clientStream = client.GetStream();//    var remoteStream = remoteClient.GetStream();//    await Task.WhenAny(ForwardData(clientStream, remoteStream),//                       ForwardData(remoteStream, clientStream));//    //}//    //catch (Exception ex)//    //{//    //    Console.WriteLine($"Errore durante la connessione a {proxmoxHost}: {ex.Message}");//    //}//    client.Close();//    //Console.WriteLine("Connessione chiusa.");//}//static async Task ForwardData(Stream from, Stream to)//{//    var buffer = new byte[8192];//    int bytesRead;//    //try//    //{//    while ((bytesRead = await from.ReadAsync(buffer)) > 0)//    {//        await to.WriteAsync(buffer.AsMemory(0, bytesRead));//        await to.FlushAsync();//    }//    //}//    //catch (Exception ex)//    //{//    //    Console.WriteLine($"Errore nel forwarding: {ex.Message}");//    //}//}//static async Task DownloadSpiceFile(IBlazorDownloadFileService blazorDownloadFileService,//                                           PveClient client,//                                           string node)//{//    //    //var uri = NavigationManager.ToAbsoluteUri(string.Empty);//    //    //var ret = Vm.VmType switch//    //    //{//    //    //    VmType.Qemu => await PveClient.Nodes[Vm.Node].Qemu[Vm.VmId].Spiceproxy.GetSpiceFileVVAsync(uri.Host),//    //    //    VmType.Lxc => await PveClient.Nodes[Vm.Node].Lxc[Vm.VmId].Spiceproxy.GetSpiceFileVVAsync(uri.Host),//    //    //    _ => throw new InvalidEnumArgumentException(),//    //    //};//    //    //var lines = ret.Content.Split(["\r\n", "\n"], StringSplitOptions.None);//    //    //for (var i = 0; i < lines.Length; i++)//    //    //{//    //    //    if (lines[i].StartsWith("proxy="))//    //    //    {//    //    //        lines[i] = $"proxy={uri.Scheme}://{uri.Host}:{uri.Port}";//    //    //        break; //    //    //    }//    //    //}//    //lines.JoinAsString(Environment.NewLine)//    var content = string.Empty;//    await blazorDownloadFileService.DownloadFileFromText($"{node}.vv",//                                                         content,//                                                         Encoding.UTF8,//                                                         "application/x-virt-viewer");//}//static async Task DownloadSpiceFile(IBlazorDownloadFileService blazorDownloadFileService,//                                           string prefix)//{//    //    //var uri = NavigationManager.ToAbsoluteUri(string.Empty);//    //    //var ret = Vm.VmType switch//    //    //{//    //    //    VmType.Qemu => await PveClient.Nodes[Vm.Node].Qemu[Vm.VmId].Spiceproxy.GetSpiceFileVVAsync(uri.Host),//    //    //    VmType.Lxc => await PveClient.Nodes[Vm.Node].Lxc[Vm.VmId].Spiceproxy.GetSpiceFileVVAsync(uri.Host),//    //    //    _ => throw new InvalidEnumArgumentException(),//    //    //};//    //    //var lines = ret.Content.Split(["\r\n", "\n"], StringSplitOptions.None);//    //    //for (var i = 0; i < lines.Length; i++)//    //    //{//    //    //    if (lines[i].StartsWith("proxy="))//    //    //    {//    //    //        lines[i] = $"proxy={uri.Scheme}://{uri.Host}:{uri.Port}";//    //    //        break; //    //    //    }//    //    //}//    //lines.JoinAsString(Environment.NewLine)//    var content = string.Empty;//    await blazorDownloadFileService.DownloadFileFromText($"{prefix}.vv",//                                                         content,//                                                         Encoding.UTF8,//                                                         "application/x-virt-viewer");//}//         async Task OpenSpiceConsole()//{//    var uri = NavigationManager.ToAbsoluteUri(string.Empty);//    var ret = Vm.VmType switch//    {//        VmType.Qemu => await PveClient.Nodes[Vm.Node].Qemu[Vm.VmId].Spiceproxy.GetSpiceFileVVAsync(uri.Host),//        VmType.Lxc => await PveClient.Nodes[Vm.Node].Lxc[Vm.VmId].Spiceproxy.GetSpiceFileVVAsync(uri.Host),//        _ => throw new InvalidEnumArgumentException(),//    };//    var lines = ret.Content.Split(["\r\n", "\n"], StringSplitOptions.None);//    for (var i = 0; i < lines.Length; i++)//    {//        if (lines[i].StartsWith("proxy="))//        {//            lines[i] = $"proxy={uri.Scheme}://{uri.Host}:{uri.Port}";//            break;//        }//    }//    await BlazorDownloadFileService.DownloadFileFromText("aa.vv",//                                                         lines.JoinAsString(Environment.NewLine),//                                                         Encoding.UTF8,//                                                         "text/plan");//}
}
