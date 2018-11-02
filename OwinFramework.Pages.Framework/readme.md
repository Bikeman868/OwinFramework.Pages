The Pages area of the Owin Framework provides very scalable and flexible
methods of adding web pages and web service endpoints to any Owin Framework
application.

This Framework package provides the default implementation of many of the
interfaces defined in the core. Most applications that use the Pages functionality
will use this package. This exists as a package in case there are people who want
to write a customized version of the framework and still put the page rendering
engine on top for example.

Since the whole Owin Framework uses Dependency Injection throughout, you can
also include this package into your application and overrride any concrete
interface implementation using IoC.
