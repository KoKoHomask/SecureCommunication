using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SecureCommunication.Common;

namespace RSA加密解密
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var rSAHelper = new RSAHelper();
            rsaKey1.Text = rSAHelper.KeyToXmlString;
            rsaKey2.Text = rSAHelper.KeyToXmlStringWithPrivate;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var jmRSA = new RSAHelper(rsaKey1.Text);
            var result = jmRSA.Encrypt(Encoding.Default.GetBytes( richTextBox2.Text));
            richTextBox2.Text = Convert.ToBase64String(result);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var rSAHelper = new RSAHelper(rsaKey2.Text);
            var btArray= Convert.FromBase64String(richTextBox2.Text);
            var result = rSAHelper.Decrypt(btArray);
            richTextBox2.Text = Encoding.Default.GetString(result);
        }
    }
}
