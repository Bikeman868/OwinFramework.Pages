# OwinFramework.Pages

This NuGet package is designed to work with the Owin Framework and adds the
ability to design web pages and serve them in an extremely scaleable way. It
also serves the Ajax requests providing both the front-end JavaScript and the
back-end Restful endpoints.

Key features:
* Very flexible, powerful, decoupled and extensible architecture
* Scales to tens of thousands of pages per second per server
* Designed from the ground up for maximum efficiency and minimal CPI cycles
* Pools and reuses objects to avoid thrashing the garbage collector
* Supports multiple ways of defining elements to support all use cases. These methods can be used in any combination
* Supports packages of elements that can be installed from third parties with namespaces to avoid naming conflicts
* Works with all existing we development technologies and libraries: Angular, React, Boosstrap etc
* Integrated support for writing web services and calling them from the UI
* Good separation of concerns and modular architecture allows mix and match of components from different authors

# Key concepts

## Assemblies

Note that each project in this solution contains a `Package.cs` file that
defines the IoC needs of that assembly. We use the Ioc.Modules NuGet package
to automatically wire up IoC and satisfy these needs.

### `OwinFramework.Pages.Core`

Contains the `PagesMiddleware` class and the interfaces used by this framework. 
Any third party libraries or add ons should reference this assembly only so that 
they work against any implementation of these interfaces.

This assembly  also contains classes that fill gaps the .Net Framework and are 
used throughout this solution.

The `PagesMiddleware` only depends on `IRequestRouter`, so in theory you
could write an implementation of `IRequestRouter` then start using the `PagesMiddleware`
in your Owin pipeline, however the rest of the framework provides a very rich
set of features that you probably want.

### `OwinFramework.Pages.Framework`

This assembly contains an implementation of `IRequestRouter` that is needed by
`PagesMiddleware` and contains a framework for building components that can have
requests routed to them.

This assembly also contains managers for name resolution in namespaces, localization
of assets and other cross cutting concerns.

This assembly depends on the interfaces in `OwinFramework.Pages.Core` but very
importantly none of the other assemblies in this solution depend on it, so you
can exclude it from your application if you want and still make use of the other
parts of this solution.

### `OwinFramework.Pages.Html`

Contains all the builders and runtime support for desiging and rendering html
pages.

Note that this assembly only depends on `OwinFramework.Pages.Core`, and does not
depend directly on `OwinFramework.Pages.Framework`. However it does need certain
interfaces to be implemented and `OwinFramework.Pages.Framework` provides 
implementations of those interfaces, so if you use these assemblies together then
your application does not have to provide anything to make it all work.

### `OwinFramework.Pages.Restful`

Contains all the builders and runtime support for responding to ajax calls from
the pages in your website.

Note that this assembly only depends on `OwinFramework.Pages.Core`, and does not
depend directly on `OwinFramework.Pages.Framework`. However it does need certain
interfaces to be implemented and `OwinFramework.Pages.Framework` provides 
implementations of those interfaces, so if you use these assemblies together then
your application does not have to provide anything to make it all work.

## Element organization

When rendering html pages this framework defines a nesting structure like this:

Pages and Services directly respond to web requests and produce responses. These
are the only elements in this solution that do this. If you want to handle other
types of request (for example serving static files) then there is lots of other
middleware packages available for the Owin Framework that provide these things.

Pages have a Layout that defines the structure of the Html. The Layout contains
an arrangement of Regions, where each Region can contain another Layout. Regions
can also contain Components.

Components are the low level elements that produce fragments of Html. Layouts and
Regions also produce Html but this should be structural.

Modules and Packages are different ways of grouping elements together and do
not produce any Html at all.

The reason for having both Regions and Components is as follows:

1. Lets say you somtimes want a piece of content within a resizable area
of the page and other times you want the same content in a pop-up box. The 
Component should not have to be aware of the two use cases. To implement this you write the 
Component once, then create two Region definitions for the two use cases. In this
way the two Regions know how to do the pop-up vs inline use cases but know
nothing about when the Compoennt does, and the Component knows nothing about
whether it is displayed inline or in a popup. This is good separation of
concerns that leads to better reuse and easier maintenance.

2. The Region can be bound to a list from the data layer and will repeat the
Component or Layout that it contains. It is a more modular design to have this
repeating functionallity outside of the Component which can focus on doing one
job well.

## Extensibility

The `OwinFramework.Pages.Framework` assembly contains an fluent builder that
provides a plug-in architecture for the actual builders. This element builder
supports a fluent syntax and also a delaritive syntax for defining the Elements.
It calls the installed build engines to do the actual building.

The only things that the fluent builder in the `OwinFramework.Pages.Framework` 
assembly knows how to build are Packages and Modules. All other elements are 
built by plug in builders.

The `OwinFramework.Pages.Html` assembly contains builders for Pages, Layouts,
Regions and Components. When you install its build engine it istalls itself 
into the fluent builder so that you can use it to build Html pages.

The `OwinFramework.Pages.Restful` assembly contains a builder for Servies.
When you install its build engine it istalls itself into the fluent builder 
so that you can use it to build restful services.

After installing a build engine you can overwrite any of the builders it
installed, providing your own implementation. For example you can install
the build engine from `OwinFramework.Pages.Html` then replace the Layout
builder with your own implementation to customize the way that layouts
get built whilst keeping all of the other builders.

Bear in mind that the whole Owin Framework uses dependency injection and
IoC to wire things up, so you can replace the standard implementation of
any interface with your own version and this will be used throught.

## Third party libraries

This solution was designed from the outset to be a platform that supports
application development by composition of libraries from many sources.

This solution provides a rich set of features for defining and rendering
pages and services, but it does not provide any application layer 
functionallity. This can be provided by your application, and
can be supplemented by third party libraries that provide blocks of
functionallity needed by many applications - for example a website navigation
menu with breadcrumbs.

The key to integrating third party libraries into your application is
the Package mechanism. The Package mechanism allows you do deploy and
install a collection of Pages, Layouts, Regions, Components and Services
that share a namespace. When these are integrated into an application the
application developer can choose the namesace for each package to
avoid naming conflicts.

Components can also have dependencies on other Components. For example
you can write a Component that adds a reference to the Bootstrap css
into the head part of the page. Now any other component can specify
that it has a dependency on this Bootstrap Component. When you do this
the Bootstrap Compoennt will be included on any page that contains
dependant Components the the link in the header will only be written
once. Also, the bootstrap link will not be output on any pages that do 
not need it.

# Glossary

## Element

This is the generic term for Page, Layout, Region, Component and Service. In this 
documentation where it refers to an Element then it means one of these things.

## Asset

Refers to css styles and JavaScript functions. These Assets can be deployed by
writing them directly into the page, or they can be gathered together into one
document and served all in one go to the browser.

Static Assets refers to Assets that had a name chosen for them at design time.
Choosing unique names is easy when you write all of the Elements but is less easy
when you integrate many third party components.

Dynamic Assets refers to Assets that were not given names at design time. These
Assets will be given short random unique names at run-time. Assuming that your
website runs on multiple servers and you dont want to enable sticky sessions
on your load balancer, then these Dynamic Assets must be written into the
html.

## Module

This is a grouping of elements. The Static Assets for these Elements will be
collected together into one file and delivered to the browser on any page
that includes any of these elements. The Module can be deployed in-page or as
a link. You can also choose to collect up all Assets into a single file
disregarding module boundaries.

## Package

This is essentially a namespace. All Elements within the Package will have their
Asset names prefixed with the namespace of the Package they are in.

If you import third party Packages then you can specify a namespace when you 
register the Package to avoid naming conflicts.

## Page

Pages respond to Http requests by returning Html to the caller. Pages have
one or more Routes defined that specify which Http requests this Page should
respond to.

Pages have a Layout that defines the overall structure of the Html produced
by the page. Pages that have the same Layout can have different content in the
Regions of that Layout.

## Layout

Layouts define an arrangement of Regions.

## Region

A Region is an area of the browser window that contains content. Each region
can contain either a Component or a Layout.

Regions can be bound to a single data object or a list of objects. When the
Region is bound to a list the Region repeats its content for each object on
the list.

## Component

Components output Html to the response stream that render as visible content.
Components can also output structural Html elements as well as outputting to 
various parts of the output, including the head section.

Copmonents also produce Assets to support their feature set. These assets
are dealt with according to the module settings for the Module that contains
this Component.

## Service

Services respond to Http requests by returning any type of content. It is most
common for Services to return JSON, but they can also return any other format
that is accepted by the browser.

Services can also generate JavaScript and inject this into Pages that depend
on the Service. For example is a Component makes calls to a Service, whenever
that Component is present on the Page the Page will include a JavaScript function
to makes calls to the Service. This JavaScript Asset can be a Static Asset or
a Dynamic Asset.

## Library

Libraries are just Components that have other Components that depend on them.

Any Component can declare a dependency on another Component. When this happens 
the dependant components are automatically rendered onto the Page as needed.

Lets say for example you wrote a Component that writes a link to the Bootstrap 
css into the head section of the page. Any other component that needs Bootstrap 
can specify that it depends on the Bootstrap Component to ensure that Bootstrap 
is included on any pages that need it.

## Route

Routes are a prioritized list of filters that apply to incomming requests and
determine which Element will be responsible for producing the response.

Routes can be added to other routes to create a routing tree which is much 
more efficient than a linear list.

## Data Context

Various elements can specify that they need a specific kind of data to be
present in order to do their work. To avoid retrieving the same data multiple
times for one Page request, this framework has Context Handlers that run
once before any of the Elements are used to handle the request.

# Getting started

I strongly urge you to look at the source code for the sample websites and
read all of this document before getting started. In particular you
should review these source files:

[Package.cs](https://github.com/Bikeman868/OwinFramework.Pages/blob/master/Sample1/Package.cs)
defines the IoC needs of the application. This is where your application defines the 
parts of the Owin Framework that are used by your application. The IoC wireup will fail
if the interfaces your application depends on are not implemented by any of the installed
NuGet packages.

[Startup.cs](https://github.com/Bikeman868/OwinFramework.Pages/blob/master/Sample1/Startup.cs)
is the entry point for the application. This is where the application builds the Owin
pipeline, registers packages and build engines and specifies any custom implementations.

[DeclarativePage.cs](https://github.com/Bikeman868/OwinFramework.Pages/blob/master/Sample1/SamplePages/DeclarativePage.cs)
is an example of how to write Pages, Layouts, Regions and Components using only
attributes. You can mix this style of declaration with any of the other options, so
for example you can define Layouts using this technique but define Pages using some
other method.

[TemlatedComponent.html](https://github.com/Bikeman868/OwinFramework.Pages/blob/master/Sample1/SamplePages/TemlatedComponent.cs)
is an example of creating a component by directly writing html into a file that contains
extentions to html making it a templating language instead of plain html. Note that these
templates can be written as complete html page so that you can preview then in a browser
directly, but only the part inside the `<body>` element is interpreted by the template
compiler.

[SemiCustomPage.cs](https://github.com/Bikeman868/OwinFramework.Pages/blob/master/Sample1/SamplePages/SemiCustomPage.cs)
is an example of how to create a web page that inherits from the `Page` class in the framework
but overrides some of the methods to customize the way that the page behaves. This is
an appropriate technique for pages that have some special needs not directly provided
by the framework.

[FullCustomPage.cs](https://github.com/Bikeman868/OwinFramework.Pages/blob/master/Sample1/SamplePages/FullCustomPage.cs)
is an example of to write a class that directly implements the `IPage` interface completely
bypassing the built-in functionallity for a fully custom implementation. You can use this
technique for specific Pages, Layouts, Regions or Components and seamlessly mix these with Elements
defined using any of the other techniques.

## To start a new website from scratch follow these steps

1. In Visual Studio start a new project or type "ASP.NET Empty Web Application". This will create a project that contains very little.
2. Go to the NuGet package manager and install these packages `Owin.Framework`, `Ioc.Modules.Ninject`, `Owin.Framework.Urchin`, `Owin.Framework.Pages.Framework`, `Owin.Framework.Pages.Html`, `Microsoft.Owin.Host.SystemWeb` - note that you might have to pick a specific version of `Microsoft.Owin.Host.SystemWeb` because each version targets only specific versions of .Net, for example "`install-package Microsoft.Owin.Host.SystemWeb -version 2.1.0`".
3. Modify the `web.config` file to use the `ExtensionlessUrlHandler`. Use [this file](https://github.com/Bikeman868/OwinFramework.Pages/blob/master/Sample1/Web.config) as an example.
4. In the root of your project add a new `Startup.cs` file and add `[assembly: OwinStartup(typeof(Startup))]` under the `using` statements and above the `namespace` statement. Resolve references and make sure it compiles.
5. In the `Startup` class add a method with ths signature `public void Configuration(IAppBuilder app)`. Add the necessary `using` statements so that the code compiles.
6. Set up the Owin Framework middleware pipeline. The Owin Framework has documentation describing how to do this. You can also look at [Startup.cs](https://github.com/Bikeman868/OwinFramework.Pages/blob/master/Sample2/Startup.cs) for a working example to copy from.
7. At the end of the `Configuration` method in your `Startup` class, resolve `IFluentBuilder` from your IoC container, for example `var fluentBuilder = ninject.Get<IFluentBuilder>();`.
8. Install the Html element builders with code similar to this: `ninject.Get<OwinFramework.Pages.Html.BuildEngine>().Install(fluentBuilder);`.
9. Have the fluent builder scan your application by adding `fluentBuilder.Register(Assembly.GetExecutingAssembly());`.
10. Resolve `INameManager` from your IoC container, for example `var nameManager = ninject.Get<INameManager>();`.
11 Get the name manager to resolve all name references and bind everything together `nameManager.Bind();`.
12. Add Components, Regions, Layouts and Pages to your application using the declarative syntax illustrated in [this example](https://github.com/Bikeman868/OwinFramework.Pages/blob/master/Sample1/SamplePages/DeclarativePage.cs).
13. At this point your solution should be similar to [this one](https://github.com/Bikeman868/OwinFramework.Pages/tree/master/Sample2). Press F5 and test your pages.
14. Study the other sample applications to see what else you can do.
15. Add a `Package.cs` file so that `Ioc.Modules` knows what your application needs and can fail early with helpful error messages.
16. Read the documentation!!

# Recommended best practice

This framework is designed to be as flexible as possible because there are so many different
ways in which developers will choose to use it. If you are feeling overwhelmed by the choices
available then I recommend following these guildelines to begin with and adapt your coding
as you become more familiar.

For Modules, Pages and Layouts use the declarative syntax as illustrated by 
[this example](https://github.com/Bikeman868/OwinFramework.Pages/blob/master/Sample1/SamplePages/DeclarativePage.cs)

For Regions, use the delcatative syntax for the most part unless they need a lot of JavaScript 
or css Assets, in which case you will probably want to convert them to use the semi-custom
approach.

For Components that produce html by binding to data models then use the templating approach.
For Components that use a lot of JavaScript or write into the different parts of the page, for 
example the `<head>` section use the semi-custom approach instead.

Divide your website up into areas of functionallity that share the same Assets and define
Modules for each area. You probably want a "Navigation" module that is used by every page
on the site. You can then create other modules to avoid delivering all of the JavaScript
and css into every page, for example you might have a "Cart" module that is only used
by the pages relating to the shopping cart.

Model the data that is used by your website and create Context Handlers for each type of
data as well as collections of those data where applicable. Try not to have too many Context
Handlers or this defeats the purpose, which is runtime efficiency by identifying components
that need the same data.

Context Handler exist so that multiple components on the page that need the same data do
not each retrieve the data they need. Instead, each component defines its data needs
and the framework makes sure that data is available by loading it only once.
