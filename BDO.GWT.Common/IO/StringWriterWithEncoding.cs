using System.IO;
using System.Text;

namespace BDO.GWT.Common.IO;

public class StringWriterWithEncoding : StringWriter
{
    public StringWriterWithEncoding(Encoding encoding)
    {
        Encoding = encoding;
    }

    public override Encoding Encoding { get; }
}
