using System.Collections.Generic;
using System.IO;
using System.Text;
using Moq.Modules;
using NUnit.Framework;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Html.Templates.Text;

namespace OwinFramework.Pages.UnitTests.Html.Templates
{
    [TestFixture]
    public class DocumentTransformerMarkdownTests: TestBase
    {
        private IDocumentTransformer _documentTransformer;

        [SetUp]
        public void SetUp()
        {
            _documentTransformer = new DocumentTransformer(SetupMock<IStringBuilderFactory>());
        }

        [Test]
        public void ShouldParseMarkdownDocument()
        {
            const string originalMarkdown =
                "# Level 1 heading\n" +
                "Some text.\n" +
                "Some more text\n" +
                "\n" +
                "## Level 2 heading\n" +
                "Less important text";

            const string expectedHtml =
                "<h1>Level 1 heading</h1>\n"+
                "<p>Some text. Some more text</p>\n" +
                "<h2>Level 2 heading</h2>\n" + 
                "<p>Less important text</p>\n";

            var document = _documentTransformer.ParseDocument("text/x-markdown", originalMarkdown);

            var html = RenderHtml(document);

            Assert.AreEqual(expectedHtml, html);
        }

        [Test]
        public void ShouldSupportAlternateHeaders()
        {
            const string originalMarkdown =
                "Level 1 heading\n"+
                "===============\n"+
                "Some text.\n"+
                "Some more text\n"+
                "\n"+
                "Level 2 heading\n"+
                "---------------\n" +
                "Less important text";

            const string expectedHtml =
                "<h1>Level 1 heading</h1>\n" + 
                "<p>Some text. Some more text</p>\n" + 
                "<h2>Level 2 heading</h2>\n" + 
                "<p>Less important text</p>\n";

            var document = _documentTransformer.ParseDocument("text/x-markdown", originalMarkdown);

            var html = RenderHtml(document);

            Assert.AreEqual(expectedHtml, html);
        }

        [Test]
        public void ShouldParseBoldAndItalic()
        {
            const string originalMarkdown =
                "Level **1** heading\n"+
                "===============\n"+
                "Some *text*.\n"+
                "Some _more_ text\n"+
                "\n"+
                "**Level _2_ heading**\n"+
                "---------------\n" +
                "Less __important__ text";

            const string expectedHtml =
                "<h1>Level <b>1</b> heading</h1>\n"+
                "<p>Some <i>text</i>. Some <i>more</i> text</p>\n" +
                "<h2><b>Level <i>2</i> heading</b></h2>\n" + 
                "<p>Less <b>important</b> text</p>\n";

            var document = _documentTransformer.ParseDocument("text/x-markdown", originalMarkdown);

            var html = RenderHtml(document);

            Assert.AreEqual(expectedHtml, html);
        }

        [Test]
        public void ShouldParseCodeTicks()
        {
            const string originalMarkdown =
                "# Level 1 heading\n" +
                "Some `source code` here";

            const string expectedHtml =
                "<h1>Level 1 heading</h1>\n" +
                "<p>Some <span class='code'>source code</span> here</p>\n";

            var document = _documentTransformer.ParseDocument("text/x-markdown", originalMarkdown);

            var html = RenderHtml(document);

            Assert.AreEqual(expectedHtml, html);
        }

        [Test]
        [TestCase("Test _italic", "<p>Test <i>italic</i></p>\n")]
        [TestCase("Test *italic", "<p>Test <i>italic</i></p>\n")]
        [TestCase("Test __bold", "<p>Test <b>bold</b></p>\n")]
        [TestCase("Test **bold", "<p>Test <b>bold</b></p>\n")]
        [TestCase("Test __bold_", "<p>Test <b>bold</b></p>\n")]
        [TestCase("Test **bold*", "<p>Test <b>bold</b></p>\n")]
        [TestCase("Test **bold and *italic", "<p>Test <b>bold and <i>italic</i></b></p>\n")]
        public void ShouldNotBreakOnUnclosedBoldOrItalic(string markdown, string expectedHtml)
        {
            var document = _documentTransformer.ParseDocument("text/x-markdown", markdown);
            var html = RenderHtml(document);
            Assert.AreEqual(expectedHtml, html);
        }

        [Test]
        public void ShouldParseUnorderedLists()
        {
            const string originalMarkdown =
                "First paragraph\n"+
                "* italic *not a list element\n"+
                "\n"+
                "* List element 1\n"+
                "* List element 2\n"+
                "  - Sub list 1\n"+
                "  - Sub list 2\n"+
                "* List element 3";

            const string expectedHtml =
                "<p>First paragraph <i> italic </i>not a list element</p>\n" +
                "<ul>\n" + 
                "  <li>List element 1</li>\n"+
                "  <li>List element 2</li>\n"+
                "  <ul>\n"+
                "    <li>Sub list 1</li>\n"+
                "    <li>Sub list 2</li>\n"+
                "  </ul>\n"+
                "  <li>List element 3</li>\n"+
                "</ul>\n";

            var document = _documentTransformer.ParseDocument("text/x-markdown", originalMarkdown);
            var html = RenderHtml(document);
            Assert.AreEqual(expectedHtml, html);
        }

        [Test]
        public void ShouldParseOrderedLists()
        {
            const string originalMarkdown =
                "My numbered list\n"+
                "\n"+
                "1. List element 1\n"+
                "2. List element 2\n"+
                "  1. Sub list 1\n"+
                "  2. Sub list 2\n" +
                "3. List element 3";

            const string expectedHtml =
                "<p>My numbered list</p>\n" +
                "<ol>\n" +
                "  <li>List element 1</li>\n" +
                "  <li>List element 2</li>\n" +
                "  <ol>\n" +
                "    <li>Sub list 1</li>\n" +
                "    <li>Sub list 2</li>\n" +
                "  </ol>\n" +
                "  <li>List element 3</li>\n" +
                "</ol>\n";

            var document = _documentTransformer.ParseDocument("text/x-markdown", originalMarkdown);

            var html = RenderHtml(document);

            Assert.AreEqual(expectedHtml, html);
        }

        [Test]
        [TestCase("[1]\n[1]: about.html", "<p><a href='about.html'>1</a> </p>\n")]
        [TestCase("[test][1]\n[1]:about.html", "<p><a href='about.html'>test</a> </p>\n")]
        [TestCase("[test](about.html)", "<p><a href='about.html'>test</a></p>\n")]
        [TestCase("<about.html>", "<p><a href='about.html'>about.html</a></p>\n")]
        public void ShouldParseLinks(string markdown, string expectedHtml)
        {
            var document = _documentTransformer.ParseDocument("text/x-markdown", markdown);
            var html = RenderHtml(document);
            Assert.AreEqual(expectedHtml, html);
        }

        [Test]
        [TestCase("![1]\n[1]: logo.jpg", "<p><img src='logo.jpg' alt='1'> </p>\n")]
        [TestCase("![test][1]\n[1]:   logo.jpg", "<p><img src='logo.jpg' alt='test'> </p>\n")]
        [TestCase("![test](logo.jpg)", "<p><img src='logo.jpg' alt='test'></p>\n")]
        [TestCase("![](logo.jpg)", "<p><img src='logo.jpg'></p>\n")]
        public void ShouldParseImages(string markdown, string expectedHtml)
        {
            var document = _documentTransformer.ParseDocument("text/x-markdown", markdown);
            var html = RenderHtml(document);
            Assert.AreEqual(expectedHtml, html);
        }

        [Test]
        [TestCase("First paragraph\n\n---\n\nSecond paragraph", "<p>First paragraph</p>\n<hr>\n<p>Second paragraph</p>\n")]
        [TestCase("First paragraph\n\n***\n\nSecond paragraph", "<p>First paragraph</p>\n<hr>\n<p>Second paragraph</p>\n")]
        [TestCase("First paragraph\n\n___\n\nSecond paragraph", "<p>First paragraph</p>\n<hr>\n<p>Second paragraph</p>\n")]
        public void ShouldParseHorizontalRules(string markdown, string expectedHtml)
        {
            var document = _documentTransformer.ParseDocument("text/x-markdown", markdown);
            var html = RenderHtml(document);
            Assert.AreEqual(expectedHtml, html);
        }

        [Test]
        public void ShouldParseBlockQuotes()
        {
            const string originalMarkdown =
                "# Title\n"+
                "\n"+
                "Some text\n"+
                "\n"+
                "> Quoting from another source.\n"+
                "> Second line of quote.\n"+
                "\n"+
                "Back to regular text";

            const string expectedHtml =
                "<h1>Title</h1>\n" +
                "<p>Some text</p>\n" +
                "<blockquote>\n" +
                "  <p>Quoting from another source. Second line of quote.</p>\n" +
                "</blockquote>\n" +
                "<p>Back to regular text</p>\n";

            var document = _documentTransformer.ParseDocument("text/x-markdown", originalMarkdown);
            var html = RenderHtml(document);
            Assert.AreEqual(expectedHtml, html);
        }


        [Test]
        public void ShouldParseSourceCode()
        {
            const string originalMarkdown =
                "# Title\n"+
                "\n"+
                "Sample code:\n"+
                "\n"+
                "```\n"+
                "  private void Test()\n"+
                "  {\n"+
                "    Console.WriteLine(\"This is a test\");\n"+
                "  }\n"+
                "```\n"+
                "\n"+
                "Back to regular text";

            const string expectedHtml =
                "<h1>Title</h1>\n" +
                "<p>Sample code:</p>\n" +
                "<pre>\n" +
                "  private void Test()\n" +
                "  {\n" +
                "    Console.WriteLine(\"This is a test\");\n" +
                "  }\n" +
                "</pre>\n" +
                "<p>Back to regular text</p>\n";

            var document = _documentTransformer.ParseDocument("text/x-markdown", originalMarkdown);
            var html = RenderHtml(document);
            Assert.AreEqual(expectedHtml, html);
        }

        [Test]
        public void ShouldParseTables()
        {
            const string originalMarkdown =
                "# Title\n"+
                "| Default  | Left | Right | Center |\n" +
                "| C1       | C2   | C3    | C4     |\n" +
                "| -------- |:---- | -----:|:------:|\n" +
                "| **R1C1** | R1C2 | R1C3  | R1C4   |\n" +
                "  R2C1     | R2C2 | R2C3  | R2C4    \n" +
                "| _R3C1_   | R3C2 | R3C3  | R3C4   |\n";

            const string expectedHtml =
                "<h1>Title</h1>\n" +
                "<table>\n" +
                "  <thead>\n" +
                "    <tr>\n" +
                "      <td>Default</td>\n" +
                "      <td>Left</td>\n" +
                "      <td>Right</td>\n" +
                "      <td>Center</td>\n" +
                "    </tr>\n" +
                "    <tr>\n" +
                "      <td>C1</td>\n" +
                "      <td>C2</td>\n" +
                "      <td>C3</td>\n" +
                "      <td>C4</td>\n" +
                "    </tr>\n" +
                "  </thead>\n" +
                "  <tbody>\n" +
                "    <tr>\n" +
                "      <td><b>R1C1</b></td>\n" +
                "      <td align='left'>R1C2</td>\n" +
                "      <td align='right'>R1C3</td>\n" +
                "      <td align='center'>R1C4</td>\n" +
                "    </tr>\n" +
                "    <tr>\n" +
                "      <td>R2C1</td>\n" +
                "      <td align='left'>R2C2</td>\n" +
                "      <td align='right'>R2C3</td>\n" +
                "      <td align='center'>R2C4</td>\n" +
                "    </tr>\n" +
                "    <tr>\n" +
                "      <td><i>R3C1</i></td>\n" +
                "      <td align='left'>R3C2</td>\n" +
                "      <td align='right'>R3C3</td>\n" +
                "      <td align='center'>R3C4</td>\n" +
                "    </tr>\n" +
                "  </tbody>\n" +
                "</table>\n";

            var document = _documentTransformer.ParseDocument("text/x-markdown", originalMarkdown);
            var html = RenderHtml(document);
            Assert.AreEqual(expectedHtml, html);
        }

        [Test]
        public void ShouldWriteSimpleMarkdown()
        {
            var document = new DocumentElement
            {
                Children = new List<IDocumentElement>
                {
                    new ParagraphElement
                    {
                        Children = new List<IDocumentElement> 
                        { 
                            new RawTextElement { Text = "First paragraph with " },
                            new SpanElement { 
                                Styles = new Dictionary<string, string>{{"font-weight", "bold"}},
                                Children = new List<IDocumentElement>
                                { 
                                    new RawTextElement{ Text = "bold text"}
                                }
                            }
                        }
                    },
                    new BreakElement { BreakType = BreakTypes.HorizontalRule },
                    new ParagraphElement
                    {
                        Children = new List<IDocumentElement> 
                        { 
                            new RawTextElement { Text = "Second paragraph" },
                        }
                    }
                }
            };
            document.Prepare();

            const string expectedMarkdown = 
                "First paragraph with **bold text**\n" +
                "\n" +
                "---\n" +
                "\n" +
                "Second paragraph\n" +
                "\n";

            var markdown = RenderMarkdown(document);
            Assert.AreEqual(expectedMarkdown, markdown);
        }

        [Test]
        public void ShouldWriteLists()
        {
            var document = new DocumentElement
            {
                Children = new List<IDocumentElement>
                {
                    new ContainerElement
                    {
                        ContainerType = ContainerTypes.BulletList,
                        Children = new List<IDocumentElement> 
                        { 
                            new ParagraphElement
                            {
                                Children = new List<IDocumentElement> 
                                { 
                                    new RawTextElement { Text = "First bullet with " },
                                    new SpanElement { 
                                        Styles = new Dictionary<string, string>{{"font-weight", "bold"}},
                                        Children = new List<IDocumentElement>
                                        { 
                                            new RawTextElement{ Text = "bold text"}
                                        }
                                    }
                                }
                            },
                            new ParagraphElement
                            {
                                Children = new List<IDocumentElement> 
                                { 
                                    new RawTextElement { Text = "Second bullet with " },
                                    new SpanElement { 
                                        Styles = new Dictionary<string, string>{{"font-style", "italic"}},
                                        Children = new List<IDocumentElement>
                                        { 
                                            new RawTextElement{ Text = "italic text"}
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new ContainerElement
                    {
                        ContainerType = ContainerTypes.NumberedList,
                        Children = new List<IDocumentElement> 
                        { 
                            new ParagraphElement
                            {
                                Children = new List<IDocumentElement> 
                                { 
                                    new RawTextElement { Text = "First numbered" }
                                }
                            },
                            new ContainerElement
                            {
                                ContainerType = ContainerTypes.NumberedList,
                                Children = new List<IDocumentElement> 
                                { 
                                    new ParagraphElement
                                    {
                                        Children = new List<IDocumentElement> 
                                        { 
                                            new RawTextElement { Text = "First nested" }
                                        }
                                    },
                                    new ParagraphElement
                                    {
                                        Children = new List<IDocumentElement> 
                                        { 
                                            new RawTextElement { Text = "Second nested" }
                                        }
                                    },
                                }
                            },
                            new ParagraphElement
                            {
                                Children = new List<IDocumentElement> 
                                { 
                                    new RawTextElement { Text = "Second numbered" }
                                }
                            }
                        }
                    }
                }
            };
            document.Prepare();

            const string expectedMarkdown = 
                "* First bullet with **bold text**\n" +
                "* Second bullet with _italic text_\n" +
                "\n" +
                "1. First numbered\n" +
                "  1. First nested\n" +
                "  2. Second nested\n" +
                "2. Second numbered\n" +
                "\n";

            var markdown = RenderMarkdown(document);
            Assert.AreEqual(expectedMarkdown, markdown);
        }

        [Test]
        public void ShouldWriteHeadings()
        {
            var document = new DocumentElement
            {
                Children = new List<IDocumentElement>
                {
                    new HeadingElement
                    {
                        Level = 1,
                        Children = new List<IDocumentElement> 
                        { 
                            new RawTextElement { Text = "Heading level 1 with " },
                            new SpanElement { 
                                Styles = new Dictionary<string, string>{{"font-weight", "bold"}},
                                Children = new List<IDocumentElement>
                                { 
                                    new RawTextElement{ Text = "bold text"}
                                }
                            }
                        }
                    },
                    new ParagraphElement
                    {
                        Children = new List<IDocumentElement>
                        {
                            new RawTextElement { Text = "This is a paragraph 1" }
                        }
                    },
                    new ParagraphElement
                    {
                        Children = new List<IDocumentElement>
                        {
                            new RawTextElement { Text = "This is a paragraph 2" }
                        }
                    },
                    new HeadingElement
                    {
                        Level = 2,
                        Children = new List<IDocumentElement> 
                        { 
                            new RawTextElement { Text = "Heading level 2" }
                        }
                    },
                    new ParagraphElement
                    {
                        Children = new List<IDocumentElement>
                        {
                            new RawTextElement { Text = "This is a paragraph 3" }
                        }
                    }
                }
            };
            document.Prepare();

            const string expectedMarkdown = 
                "# Heading level 1 with **bold text**\n" +
                "This is a paragraph 1\n" +
                "\n" +
                "This is a paragraph 2\n" +
                "\n" +
                "## Heading level 2\n" +
                "This is a paragraph 3\n" +
                "\n";

            var markdown = RenderMarkdown(document);
            Assert.AreEqual(expectedMarkdown, markdown);
        }

        [Test]
        public void ShouldWriteLinksAndImages()
        {
            var document = new DocumentElement
            {
                Children = new List<IDocumentElement>
                {
                    new ParagraphElement
                    {
                        Children = new List<IDocumentElement>
                        {
                            new RawTextElement { Text = "Paragraph 1. " },
                            new AnchorElement 
                            { 
                                LinkAddress = "http://www.stockhouse.com/",
                                Children = new List<IDocumentElement>
                                {
                                    new RawTextElement{Text = "Reference "},
                                    new SpanElement
                                    { 
                                        Styles = new Dictionary<string, string>{{"font-weight", "bold"}},
                                        Children = new List<IDocumentElement>
                                        {
                                            new RawTextElement { Text = "text" }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new ParagraphElement
                    {
                        Children = new List<IDocumentElement>
                        {
                            new RawTextElement { Text = "Paragraph 2. " },
                            new AnchorElement 
                            { 
                                LinkAddress = "http://www.stockhouse.com/"
                            }
                        }
                    },
                    new ParagraphElement
                    {
                        Children = new List<IDocumentElement>
                        {
                            new RawTextElement { Text = "Paragraph 3. " },
                            new ImageElement 
                            { 
                                LinkAddress = "/images/icon.png"
                            }
                        }
                    },
                    new ParagraphElement
                    {
                        Children = new List<IDocumentElement>
                        {
                            new RawTextElement { Text = "Paragraph 4. " },
                            new ImageElement 
                            { 
                                LinkAddress = "/images/icon.png",
                                AltText = "Icon"
                            }
                        }
                    },
                }
            };
            document.Prepare();

            const string expectedMarkdown =
                "Paragraph 1. [Reference text](http://www.stockhouse.com/)\n" +
                "\n" +
                "Paragraph 2. <http://www.stockhouse.com/>\n" +
                "\n" +
                "Paragraph 3. ![](/images/icon.png)\n" +
                "\n" +
                "Paragraph 4. ![Icon](/images/icon.png)\n" +
                "\n";

            var markdown = RenderMarkdown(document);
            Assert.AreEqual(expectedMarkdown, markdown);
        }

        [Test]
        public void ShouldWritePreformattedText()
        {
            var document = new DocumentElement
            {
                Children = new List<IDocumentElement>
                {
                    new HeadingElement
                    {
                        Level = 3,
                        Children = new List<IDocumentElement> 
                        { 
                            new RawTextElement { Text = "Code sample" }
                        }
                    },
                    new ContainerElement
                    {
                        ContainerType = ContainerTypes.PreFormatted,
                        Children = new List<IDocumentElement>
                        {
                            new ParagraphElement
                            {
                                Children = new List<IDocumentElement>
                                {
                                    new RawTextElement { Text = "public class MyClass {" },
                                }
                            },
                            new ParagraphElement
                            {
                                Children = new List<IDocumentElement>
                                {
                                    new RawTextElement { Text = "  public void TestMethod() {" },
                                }
                            },
                            new ParagraphElement
                            {
                                Children = new List<IDocumentElement>
                                {
                                    new RawTextElement { Text = "    DoSomething();" },
                                }
                            },
                            new ParagraphElement
                            {
                                Children = new List<IDocumentElement>
                                {
                                    new RawTextElement { Text = "  }" },
                                }
                            },
                            new ParagraphElement
                            {
                                Children = new List<IDocumentElement>
                                {
                                    new RawTextElement { Text = "}" },
                                }
                            }
                        }
                    }
                }
            };
            document.Prepare();

            const string expectedMarkdown =
                "### Code sample\n" +
                "\n" +
                "```\n" +
                "public class MyClass {\n" +
                "  public void TestMethod() {\n" +
                "    DoSomething();\n" +
                "  }\n" +
                "}\n" +
                "```\n" +
                "\n";

            var markdown = RenderMarkdown(document);
            Assert.AreEqual(expectedMarkdown, markdown);
        }

        [Test]
        public void ShouldWriteBlockQuotes()
        {
            var document = new DocumentElement
            {
                Children = new List<IDocumentElement>
                {
                    new HeadingElement
                    {
                        Level = 1,
                        Children = new List<IDocumentElement> 
                        { 
                            new RawTextElement { Text = "Sample quote" }
                        }
                    },
                    new ContainerElement
                    {
                        ContainerType = ContainerTypes.BlockQuote,
                        Children = new List<IDocumentElement>
                        {
                            new ParagraphElement
                            {
                                Children = new List<IDocumentElement>
                                {
                                    new RawTextElement { Text = "The quick brown fox" },
                                }
                            },
                            new ParagraphElement
                            {
                                Children = new List<IDocumentElement>
                                {
                                    new RawTextElement { Text = "jumped over the " },
                                    new SpanElement { 
                                        Styles = new Dictionary<string, string>{{"font-weight", "bold"}},
                                        Children = new List<IDocumentElement>
                                        { 
                                            new RawTextElement{ Text = "lazy"}
                                        }
                                    },
                                    new RawTextElement { Text = " dog" },
                                }
                            }
                        }
                    },
                    new ContainerElement
                    {
                        ContainerType = ContainerTypes.BlockQuote,
                        Children = new List<IDocumentElement>
                        {
                            new ParagraphElement
                            {
                                Children = new List<IDocumentElement>
                                {
                                    new RawTextElement { Text = "The quick brown fox jumped over the lazy dog. " },
                                    new RawTextElement { Text = "Peter piper picked a peck of pickled pepper." }
                                }
                            }
                        }
                    }
                }
            };
            document.Prepare();

            const string expectedMarkdown =
                "# Sample quote\n" +
                "\n" +
                "> The quick brown fox\n" +
                "> jumped over the **lazy** dog\n" +
                "\n" +
                "> The quick brown fox jumped over the lazy dog. Peter piper picked\n" +
                "> a peck of pickled pepper.\n" +
                "\n";

            var markdown = RenderMarkdown(document);
            Assert.AreEqual(expectedMarkdown, markdown);
        }

        [Test]
        public void ShouldWriteTables()
        {
            var document = new DocumentElement
            {
                Children = new List<IDocumentElement>
                {
                    new ContainerElement
                    {
                        ContainerType = ContainerTypes.Table,
                        Children = new List<IDocumentElement> 
                        {
                            new ContainerElement
                            {
                                ContainerType = ContainerTypes.TableHeader,
                                Children = new  List<IDocumentElement>
                                {
                                    new ContainerElement
                                    {
                                        ContainerType = ContainerTypes.TableHeaderRow,
                                        Children = new  List<IDocumentElement>
                                        {
                                            new ContainerElement
                                            {
                                                ContainerType = ContainerTypes.TableDataCell,
                                                Attributes = new Dictionary<string, string>{{"align", "left"}},
                                                Children = new  List<IDocumentElement>
                                                {
                                                    new RawTextElement { Text = "Column 1" }
                                                }
                                            },
                                            new ContainerElement
                                            {
                                                ContainerType = ContainerTypes.TableDataCell,
                                                Children = new  List<IDocumentElement>
                                                {
                                                    new RawTextElement { Text = "Column 2" }
                                                }
                                            },
                                            new ContainerElement
                                            {
                                                ContainerType = ContainerTypes.TableDataCell,
                                                Children = new  List<IDocumentElement>
                                                {
                                                    new RawTextElement { Text = "Column 3" }
                                                }
                                            },
                                        }
                                    },
                                }
                            },
                            new ContainerElement
                            {
                                ContainerType = ContainerTypes.TableBody,
                                Children = new  List<IDocumentElement>
                                {
                                    new ContainerElement
                                    {
                                        ContainerType = ContainerTypes.TableDataRow,
                                        Children = new  List<IDocumentElement>
                                        {
                                            new ContainerElement
                                            {
                                                ContainerType = ContainerTypes.TableDataCell,
                                                Attributes = new Dictionary<string, string>{{"align", "center"}},
                                                Children = new  List<IDocumentElement>
                                                {
                                                    new RawTextElement { Text = "R1C1" }
                                                }
                                            },
                                            new ContainerElement
                                            {
                                                ContainerType = ContainerTypes.TableDataCell,
                                                Attributes = new Dictionary<string, string>{{"align", "center"}},
                                                Children = new  List<IDocumentElement>
                                                {
                                                    new RawTextElement { Text = "R1C2" }
                                                }
                                            },
                                            new ContainerElement
                                            {
                                                ContainerType = ContainerTypes.TableDataCell,
                                                Children = new  List<IDocumentElement>
                                                {
                                                    new RawTextElement { Text = "R1C3" }
                                                }
                                            },
                                        }
                                    },
                                    new ContainerElement
                                    {
                                        ContainerType = ContainerTypes.TableDataRow,
                                        Children = new  List<IDocumentElement>
                                        {
                                            new ContainerElement
                                            {
                                                ContainerType = ContainerTypes.TableDataCell,
                                                Children = new  List<IDocumentElement>
                                                {
                                                    new RawTextElement { Text = "R2C1" }
                                                }
                                            },
                                            new ContainerElement
                                            {
                                                ContainerType = ContainerTypes.TableDataCell,
                                                Children = new  List<IDocumentElement>
                                                {
                                                    new RawTextElement { Text = "R2C2" }
                                                }
                                            },
                                            new ContainerElement
                                            {
                                                ContainerType = ContainerTypes.TableDataCell,
                                                Attributes = new Dictionary<string, string>{{"align", "right"}},
                                                Children = new  List<IDocumentElement>
                                                {
                                                    new RawTextElement { Text = "R2C3" }
                                                }
                                            },
                                        }
                                    },
                                    new ContainerElement
                                    {
                                        ContainerType = ContainerTypes.TableDataRow,
                                        Children = new  List<IDocumentElement>
                                        {
                                            new ContainerElement
                                            {
                                                ContainerType = ContainerTypes.TableDataCell,
                                                Children = new  List<IDocumentElement>
                                                {
                                                    new RawTextElement { Text = "R3C1" }
                                                }
                                            },
                                            new ContainerElement
                                            {
                                                ContainerType = ContainerTypes.TableDataCell,
                                                Children = new  List<IDocumentElement>
                                                {
                                                    new RawTextElement { Text = "R3C2" }
                                                }
                                            },
                                            new ContainerElement
                                            {
                                                ContainerType = ContainerTypes.TableDataCell,
                                                Children = new  List<IDocumentElement>
                                                {
                                                    new RawTextElement { Text = "R3C3" }
                                                }
                                            },
                                        }
                                    },
                                }
                            }
                        }
                    }
                }
            };
            document.Prepare();

            const string expectedMarkdown =
                "| Column 1 | Column 2 | Column 3 |\n" +
                "|:--- |:---:| ---:|\n" +
                "| R1C1 | R1C2 | R1C3 |\n" +
                "| R2C1 | R2C2 | R2C3 |\n" +
                "| R3C1 | R3C2 | R3C3 |\n" +
                "\n";

            var markdown = RenderMarkdown(document);
            Assert.AreEqual(expectedMarkdown, markdown);
        }

        private string RenderHtml(IDocumentElement document)
        {
            var output = new StringBuilder();
            using (var textWriter = new StringWriter(output))
            {
                _documentTransformer.FormatDocument("text/html", textWriter, document);
            }
            return output.ToString();
        }

        private string RenderMarkdown(IDocumentElement document)
        {
            var output = new StringBuilder();
            using (var textWriter = new StringWriter(output))
            {
                _documentTransformer.FormatDocument("text/x-markdown", textWriter, document);
            }
            return output.ToString();
        }

    }
}
