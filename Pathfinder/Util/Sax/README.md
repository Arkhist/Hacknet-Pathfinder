# SAX.Net

SAX.Net is a .NET port of the SAX (http://www.saxproject.org/) Java library.

SAX is the Simple API for XML.

## Installation

To install [SAX.Net](https://www.nuget.org/packages/sax.net) from the [NuGet Gallery](http://www.nuget.org), run the following in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console)
```powershell
PM> Install-Package SAX.Net
```

# Configuration

``` XML
<configSections>
  <section name="sax" type="Sax.Net.Helpers.SaxConfigurationSection, Sax.Net"/>
</configSections>
<!-- set either but not both -->
<sax xmlReaderType="" xmlReaderFactoryType=""/>
```

# Usage

A SAX2-compatible XML parser is required e.g. [Ælfred.Net](http://www.github.com/rasmusjp/aelfred.net) or [TagSoup.Net](http://www.github.com/rasmusjp/tagsoup.net) for HTML

```C#
using System;
using System.IO;

using Sax.Net;
using Sax.Net.Helpers;

public class MySAXApp : DefaultHandler {
  public static void Main(params string[] args) {
    var handler = new MySAXApp();
    IXmlReader parser = XmlReaderFactory.Current.CreateXmlReader();
    parser.ContentHandler = handler;
    parser.ErrorHandler = handler;

    foreach(string fileName in args) {
      using(StreamReader reader = new StreamReader(fileName)) {
        parser.Parse(new InputSource(reader));
      }
    }
  }

  public override void StartDocument() {
    Console.WriteLine("Start document");
  }

  public override void EndDocument() {
    Console.WriteLine("End document");
  }

  public override void StartElement(string uri, string localName, string qName, IAttributes atts) {
    if (string.IsNullOrEmpty(uri)) {
      Console.WriteLine("Start element: {0}", qName);
    } else {
      Console.WriteLine("Start element: {{{0}}}{1}", uri, localName);
    }
  }

  public override void EndElement(string uri, string localName, string qName) {
    if (string.IsNullOrEmpty(uri)) {
      Console.WriteLine("End element: {0}", qName);
    } else {
      Console.WriteLine("End element: {{{0}}}{1}", uri, localName);
    }
  }

  public override void Characters(char[] ch, int start, int length)  {
    Console.Write("Characters:    \"");
    for (int i = start; i < start + length; i++) {
      switch (ch[i]) {
        case '\\':
          Console.Write("\\\\");
          break;
        case '"':
          Console.Write("\\\"");
          break;
        case '\n':
          Console.Write("\\n");
          break;
        case '\r':
          Console.Write("\\r");
          break;
        case '\t':
          Console.Write("\\t");
          break;
        default:
          Console.Write(ch[i]);
          break;
      }
    }
    Console.Write("\"\n");
  }
}
```

Given the following input:
```XML
<?xml version="1.0"?>

<poem xmlns="http://www.megginson.com/ns/exp/poetry">
<title>Roses are Red</title>
<l>Roses are red,</l>
<l>Violets are blue;</l>
<l>Sugar is sweet,</l>
<l>And I love you.</l>
</poem>
```

The output would look something like this:
```
Start document
Start element: {http://www.megginson.com/ns/exp/poetry}poem
Characters:    "\n"
Start element: {http://www.megginson.com/ns/exp/poetry}title
Characters:    "Roses are Red"
End element:   {http://www.megginson.com/ns/exp/poetry}title
Characters:    "\n"
Start element: {http://www.megginson.com/ns/exp/poetry}l
Characters:    "Roses are red,"
End element:   {http://www.megginson.com/ns/exp/poetry}l
Characters:    "\n"
Start element: {http://www.megginson.com/ns/exp/poetry}l
Characters:    "Violets are blue;"
End element:   {http://www.megginson.com/ns/exp/poetry}l
Characters:    "\n"
Start element: {http://www.megginson.com/ns/exp/poetry}l
Characters:    "Sugar is sweet,"
End element:   {http://www.megginson.com/ns/exp/poetry}l
Characters:    "\n"
Start element: {http://www.megginson.com/ns/exp/poetry}l
Characters:    "And I love you."
End element:   {http://www.megginson.com/ns/exp/poetry}l
Characters:    "\n"
End element:   {http://www.megginson.com/ns/exp/poetry}poem
End document
```

# License

SAX.Net is licensed under [LGPL V3](LICENSE).
