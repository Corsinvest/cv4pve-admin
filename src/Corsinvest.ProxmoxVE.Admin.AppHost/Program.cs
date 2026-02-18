/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
var builder = DistributedApplication.CreateBuilder(args);

//var mailpit = builder.AddMailPit("mailpit");

var postgres = builder.AddPostgres("postgres", port: 5432)
                      .WithContainerName("postgres-cv4pve-admin")
                      .WithDataVolume()
                      //.WithPgAdmin()
                      .WithLifetime(ContainerLifetime.Persistent)
                      .WithPgWeb();

var cv4pveDb = postgres.AddDatabase("DefaultConnection", "cv4pve-admin");

builder.AddProject<Projects.Corsinvest_ProxmoxVE_Admin>("cv4pve-admin")
       .WithOtlpExporter()
       .WithReference(cv4pveDb)
       .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
       .WaitFor(postgres)
       //.WithReference(mailpit)
       ;

builder.Build().Run();
