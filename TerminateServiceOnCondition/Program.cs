﻿using TerminateServiceOnCondition;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddTransient<IResourceChecksum, FileResourceChecksum>();
builder.Services.AddSingleton<Func<string, IResourceChecksum>>(_ => res => new FileResourceChecksum(res));
builder.Services.AddHostedService<ResourceWatcher>(sp => {
  // TODO read from command line arguments or configuration
  var resources = new [] {
    "file1.txt", 
    "file2.txt", 
    "file3.txt"
  };
  return ActivatorUtilities.CreateInstance<ResourceWatcher>(sp, [resources]);
});

var host = builder.Build();
host.Run();