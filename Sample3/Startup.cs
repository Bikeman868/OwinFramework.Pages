using Microsoft.Owin;

// Uncomment only one of the lines below to test that particular use case

//[assembly: OwinStartup(typeof(Sample3.UseCase1.Startup))]
//[assembly: OwinStartup(typeof(Sample3.UseCase2.Startup))]
//[assembly: OwinStartup(typeof(Sample3.UseCase3.Startup))]
//[assembly: OwinStartup(typeof(Sample3.UseCase4.Startup))]
[assembly: OwinStartup(typeof(Sample3.UseCase5.Startup))]
//[assembly: OwinStartup(typeof(Sample3.UseCase6.Startup))]
