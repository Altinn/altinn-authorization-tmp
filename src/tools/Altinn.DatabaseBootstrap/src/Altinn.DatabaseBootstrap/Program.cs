// See https://aka.ms/new-console-template for more information
using System.CommandLine;

var rgName = new Option<string>(
    name: "--rg-name",
    description: "ARM resource group name"
);

var dbName = new Option<string>(
    name: "--db-name",
    description: "whole just ARM resource name"
);

var cmd = new RootCommand("Database Bootstrapper Tool");
cmd.AddOption(rgName);
cmd.AddOption(dbName);
