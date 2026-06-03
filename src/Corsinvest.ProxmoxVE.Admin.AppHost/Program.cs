/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
var builder = DistributedApplication.CreateBuilder(args);

//var mailpit = builder.AddMailPit("mailpit");

// Pinned to 17: Postgres 18+ changed PGDATA layout (subdirectory per major
// version) and refuses to start against volumes created by 17.x without an
// explicit pg_upgrade. Bump only after a tested migration.
var postgres = builder.AddPostgres("postgres", port: 5432)
                      .WithImageTag("17")
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
