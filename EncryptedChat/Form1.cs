using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace EncryptedChat
{
    public partial class Form1 : Form
    {
        string key = string.Empty;
        string Uid = string.Empty;
        string mdUid = string.Empty;
        string dateUid = string.Empty;
        string keyTwo = "abcdefghijuklmno0123456789012345";
        List<string> msgDates = new List<string>();
        HttpHandler HttpDo = new HttpHandler();
        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            key = txtKey.Text;
            Uid = txtUid.Text;
            mdUid = MD5(Uid);
            dateUid = MD5(mdUid);

            toLog("Connecting...");
            string textmsg = getContent(mdUid);
            if (textmsg == null || textmsg == "CLOSED")
            {
                toLog("Connected!");
                HttpDo.addPost(mdUid, "CLOSED", key);
                
            }
            else
            {
                toLog("Detected UnClosed connection!");
                toLog("Connected!");
            }
            InitTimer();
            btnClose.Enabled = true;
            btnSend.Enabled = true;
        }

        private void toLog(string p)
        {
            txtLog.Text += (p + "\r\n");
        }


        private  string getContent(string id)
        {
            HttpHandler httpDo = new HttpHandler();
            if (httpDo.getPost(id) == "Error - Not found")
            {
                return null;
            }
            string content = Decrypt(httpDo.getPost(id), key);
            string c = null;
            try
            {
                c = content.TrimEnd('\0'); //removes null bytes that comes with string
            }
            catch (NullReferenceException) { }

            return c;
        }
        public static string Decrypt(string toDecrypt, string key)
        {
            if (toDecrypt == "Error - Not found")
            {
                return null;
            }
            if (toDecrypt != null)
            {

                byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key); // AES-256 key
                byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);
                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray;
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.None; // better lang support
                ICryptoTransform cTransform = rDel.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return UTF8Encoding.UTF8.GetString(resultArray);
            }
            else { return null; }
        } 

        private static string MD5(string phase)
        {

            if (phase != null)
            {
                // byte array representation of that string
                byte[] encodedPassword = new UTF8Encoding().GetBytes(phase);

                // need MD5 to calculate the hash
                byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);

                // string representation (similar to UNIX format)
                string encoded = BitConverter.ToString(hash)
                    // without dashes
                   .Replace("-", string.Empty)
                    // make lowercase
                   .ToLower();
                return encoded;
            }
            return null;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            HttpDo.addPost(mdUid, "CLOSED", key);
            toLog("closed!");
            timer1.Stop();
            btnClose.Enabled = false;
            btnSend.Enabled = false;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            PostMsg(txtMsg.Text);
            txtMsg.Text = string.Empty;
        }
        private Timer timer1;
        public void InitTimer()
        {
            timer1 = new Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000; // in miliseconds
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
             
         CheckMessage();
             
       }

        private void CheckMessage()
        {
            string latest = getContent(dateUid);
            string textmsg = getContent(mdUid);
            if (latest != null)
            {
                if (!msgDates.Exists(element => element == latest) && textmsg != "CLOSED")
                {
                    msgDates.Add(latest);
                    toLog(latest.Substring(0, 16) + " " + textmsg);
                }
            }
        }
        private void PostMsg(string text)
        {
            string date = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            HttpDo.addPost(mdUid, text, key);
            HttpDo.addPost(dateUid, date, key);
            msgDates.Add(date);
        }

        private void btnclear_Click(object sender, EventArgs e)
        {
            txtLog.Text = string.Empty;
        }


        public static string cEncrypt(string input, string key)
        {
            byte[] inputArray = UTF8Encoding.UTF8.GetBytes(input);
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.None;
            ICryptoTransform cTransform = tripleDES.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        public static string cDecrypt(string input, string key)
        {
            try
            {
                input = input.TrimEnd('\0'); //removes null bytes that comes with string
            }
            catch (NullReferenceException) { }
            try
            {
                byte[] inputArray = Convert.FromBase64String(input);
                TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
                tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
                tripleDES.Mode = CipherMode.ECB;
                tripleDES.Padding = PaddingMode.None;
                ICryptoTransform cTransform = tripleDES.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                tripleDES.Clear();
                return UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch (System.FormatException)
            {
                return input;
            }
        }
    }
}
