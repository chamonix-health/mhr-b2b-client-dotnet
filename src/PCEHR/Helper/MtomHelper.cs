#if NETSTANDARD2_0
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Nehta.VendorLibrary.PCEHR
{
    /// <summary>
    /// Utility class to return an XmlMtomWriter or XmlMtomReader from the CoreWCF library instead of using
    /// the XmlDictionaryWriter.CreateMtomWriter and XmlDictionaryReader.CreateMtomReader static methods,
    /// which don't work in more recent versions of the core libraries.
    /// </summary>
    /// <remarks>Since the XmlMtomWriter and XmlMtomReader classes are internal, reflection is used to create an instance.</remarks>
    internal class MtomHelper
    {
        private const string XmlMtomWriterTypeName = "CoreWCF.Xml.XmlMtomWriter";
        private const string XmlMtomReaderTypeName = "CoreWCF.Xml.XmlMtomReader";

        // Any CoreWCF type to get an Assembly reference
        private static Assembly assembly = typeof(CoreWCF.DBNull).Assembly;

        private static Type writerType = assembly.GetType(XmlMtomWriterTypeName, true);
        private static Type readerType = assembly.GetType(XmlMtomReaderTypeName, true);

        private static Type[] setOutputSignature = new[] { typeof(Stream), typeof(Encoding), typeof(int), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(bool) };
        private static MethodInfo setOutput = writerType.GetMethod("SetOutput", BindingFlags.Instance | BindingFlags.Public, null, setOutputSignature, null);

        private static Type[] setInputSignature = new[] { typeof(byte[]), typeof(int), typeof(int), typeof(Encoding[]), typeof(string), typeof(XmlDictionaryReaderQuotas), typeof(int), typeof(OnXmlDictionaryReaderClose) };
        private static MethodInfo setInput = readerType.GetMethod("SetInput", BindingFlags.Instance | BindingFlags.Public, null, setInputSignature, null);

        /// <inheritdoc cref="System.Xml.XmlDictionaryWriter.CreateMtomWriter(Stream, Encoding, int, string, string, string, bool, bool)" />
        public static XmlDictionaryWriter CreateMtomWriter(Stream stream, Encoding encoding, int maxSizeInBytes, string startInfo, string boundary, string startUri, bool writeMessageHeaders, bool ownsStream)
        {
            var writer = (XmlDictionaryWriter)Activator.CreateInstance(writerType);
            setOutput.Invoke(writer, new object[] { stream, encoding, maxSizeInBytes, startInfo, boundary, startUri, writeMessageHeaders, ownsStream });
            return writer;
        }

        /// <inheritdoc cref="System.Xml.XmlDictionaryReader.CreateMtomReader(byte[], int, int, Encoding, XmlDictionaryReaderQuotas)" />
        public static XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, Encoding encoding, XmlDictionaryReaderQuotas quotas)
        {
            var reader = (XmlDictionaryReader)Activator.CreateInstance(readerType);
            setInput.Invoke(reader, new object[] { buffer, offset, count, new Encoding[1] { encoding }, null, quotas, int.MaxValue, null });
            return reader;
        }
    }
}
#endif
