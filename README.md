# OwinFramework.Pages

# Glossary

## Element

This is the generic term for Page, Layout, Region, Component and Service. In this 
documentation where it refers to am Element then it means one of these things.

## Asset

Refers to css styles and JavaScript functions. These assets can be deployed by
writing them directly into the page, or they can be gathered together into one
document and served all in one go to the browser.

Static Assets refers to Assets that had a name chosen for them at design time.
Choosing unique names is easy when you write all of the Elements but is less easy
when you integrate many third party components.

Dynamic Assets refers to Assets that were not given names at design time. These
Assets will be given short random unique names at run-time. Assuming that your
website runs on multiple servers, these Dynamic Assets must be written into the
html. We are assuming that you want to avoid sticky sessions on the load balancer.

## Module

This is a grouping of elements. The Static Assets for these Elements will be
collected together into one file and delivered to the browser on any page
that includes any of these elements. The Module can be deployed in-page or as
a link.You can also choose to collect up all Assets into a single file
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

Regions can be bound to a single data object or a list of obejcts. When the
Region is bound to a list the Region repeats its content for each object on
the list.

## Component

Components output Html to the response stream that render as visible content.
Components can also output structural Html elements as well as outputting to 
various parts of the output, including the head section.

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

Libraries are just Components that are used in a specific way. Any Component
can declare a dependency on another Component. When this happens the dependant
components are rendered onto the Page. Lets say for example you wrote a
Component that writes a link to the Bootstrap css into the head section of the
page, then any other component can specify that it depends on it so ensure
that Bootstrap is included on any pages than need it.

## Route

Routes are a prioritized list of filters that apply to incomming requests and
determine which Element will be responsible for producing the response.

Routes can be added to other routes to create a routing tree which is much 
more efficient then a linear list.

## Data Context

Various elements can specify that they need a specific kind of data to be
present in order to do their work. To avoid retrieving the same data multiple
times for one Page request, this framework has Context Handlers that run
once before any of the Elements are used to handle the request.


