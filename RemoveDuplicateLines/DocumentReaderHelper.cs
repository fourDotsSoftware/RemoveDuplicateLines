using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace RemoveDuplicateLines
{
    public class DocumentReaderHelper
    {
        public static string ReadRTFDocument(string filepath)
        {
            try
            {
                RichTextBox rtb = new RichTextBox();
                rtb.LoadFile(filepath);

                return rtb.Text;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool BusyWord = false;

        public static string ReadWordDocument(string filepath)
        {
            try
            {

                BusyWord = true;

                object doc = null;
                object oText = null;
                object oContent = null;
                object oDocuments = null;

                object missing = System.Reflection.Missing.Value;

                try
                {
                    System.Globalization.CultureInfo oldCI = System.Threading.Thread.CurrentThread.CurrentCulture;

                    try
                    {
                        OfficeHelper.CreateWordApplication();

                        object document_filepath = filepath;

                        oDocuments = OfficeHelper.WordApp.GetType().InvokeMember("Documents", BindingFlags.InvokeMethod | BindingFlags.GetProperty, null, OfficeHelper.WordApp, null);

                        doc = oDocuments.GetType().InvokeMember("Open", BindingFlags.InvokeMethod | BindingFlags.GetProperty, null, oDocuments, new object[] { document_filepath });

                        System.Threading.Thread.Sleep(100);
                    }
                    catch (Exception exword)
                    {
                        throw (exword);
                    }

                    oContent = doc.GetType().InvokeMember("Content", BindingFlags.InvokeMethod | BindingFlags.GetProperty, null, doc, null);
                    oText = oContent.GetType().InvokeMember("Text", BindingFlags.InvokeMethod | BindingFlags.GetProperty, null, oContent, null);

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    return oText.ToString();

                }
                catch (Exception exmain)
                {
                    throw (exmain);
                }
                finally
                {
                    try
                    {
                        doc.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, doc, null);
                    }
                    catch { }

                    doc = null;
                    oContent = null;
                    oText = null;
                    oDocuments = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    OfficeHelper.QuitWordApplication();

                    OfficeHelper.QuitOfficeApplications();
                }
            }
            finally
            {
                BusyWord = false;
            }
        }
    }
}
