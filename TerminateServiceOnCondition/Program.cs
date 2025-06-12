using TerminateServiceOnCondition;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<FileWatcher>(sp => {
  // TODO read from command line arguments or configuration
  var files = new [] {
    "file1.txt", 
    "file2.txt", 
    "file3.txt"
  };
  return ActivatorUtilities.CreateInstance<FileWatcher>(sp, [files]);
});

var host = builder.Build();
host.Run();