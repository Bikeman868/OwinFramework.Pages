To use this middleware add it to your Owin pipeline then append `?debug=true` to the url of your page.
This debug middleware will handle the request, locate the page to be rendered then output debug information instead of rendering the page.

You can set the `Accept` header to get the debug information in a number of formats including: JSON, XML, Html amd SVG. You can
also pass one of these values in the query string, for example ?debug=svg
