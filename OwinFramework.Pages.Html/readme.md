The Pages area of the Owin Framework provides very scalable and flexible
methods of adding web pages and web service endpoints to any Owin Framework
application.

This Html module contains the functionallity for rendering Html. It provides
many different ways of doing this including:

* Writing custom elements that implement the `IComponent`, `IRegion`, `ILayout`,
  `IPage` etc interfaces.
* Writing custom elements that inherit from the `Component`, `Region`, `Layout`,
  `Page` etc classes and override virtual methods to create applicatiob features.
* Decorating classes with attributes to define their behavior, reflecting over
  assemblies to find these classes and register them with the rendering engine.
* Using the `FluentBuilder` to define components, regions, layouts and pages
  using a fluent syntax in C#.
* Creating a package class that uses the fluent builder to construct a set of
  elements that can be imported and reused in different applications.
* Loading and parsing templates. There are a number of loaders provided, including
  loading templates from a Url or by scanning the file system. There are a number
  of parsers including Html, Markdown and Angular. These template parsers support
  data binding, conditional inclusion of content and repeating.
* Using the fluent template builder to programatically construct and register
  templates from data - for example from a database.
