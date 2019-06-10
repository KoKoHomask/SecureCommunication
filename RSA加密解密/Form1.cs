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
        //RSAHelper rSAHelper;
        //RSAHelper jmRSA;
        string str = "<RSAKeyValue><Modulus>taajTLZGoukf5omEinyiyHuzH4Er7j5Won/WF51qKZ2V3WQyPhraFb7f1S7Qv1KXSqkLR4U4mOoKGfN7GEJhvlAsRCjCqWuSGN7AIOTAyF255Xf4WfarJW/gVgQ9NU9d33455yByM1Dzh3UxINdWzBQ3xAYtbPg/OVZ5YDTnFFU=</Modulus><Exponent>AQAB</Exponent><P>wbCIS9buGCK/eMoz3A7szuXwEV/dYoP1eAW/Pd9htY+vMu5qLpK8Yd9XNlQ4Hw8eFIVyZBDJ5AfIKmApQ+69zw==</P><Q>8Bam/6fobGZ4CWjPQ9RrAmNzbHLj9XnEjWORcSaz/taW74x1Gq/MqBtZ5qzgeeLcgxIJncEVT72OkpK1tfVYmw==</Q><DP>RA95RVUIIzkKmTJMWNZxxh4sZp6OF5ERD5TNZ7t4BSmxQa9fZnAvfCDT74ZWlitkwYdG8/ymRNerMaDKv1FEJw==</DP><DQ>5vOzc6zOnTA4+jwzXTmCMsr+Jm2isemP1L7aEvG7JkLA8PC5WZGRGoktAwR772P24bLBQ1qQqKNc+cxdNgssTw==</DQ><InverseQ>gjQl1+iNcV4uO9hm+SPFtpiSF+wyg9GnzbT3f9AchTtXi9s8aNe+TZcIOwbq5jc6cI57/lNEdaLzV3QOoKXztg==</InverseQ><D>Qkh+Cf3xJH0AoPTJImd8Vr+ciwBtcU/Z+Q3Ap3lPRcQ07UYkdzkBHzI5JucgUM/oWNv6O6zHDDVKXlHUNvPyCguaQtTCbPDU8rb830H4s5rrauJGWbx891cv+wgvRx8a/iDWQhwpTm77WetwC3IJA9AABIdiiQaKmz7+L6XC37k=</D></RSAKeyValue>";
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
            //jmRSA.EncryptionPadding = System.Security.Cryptography.RSAEncryptionPadding.OaepSHA1;
            var result = jmRSA.Encrypt(Encoding.Default.GetBytes( richTextBox2.Text));
            richTextBox2.Text = Convert.ToBase64String(result);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var rSAHelper = new RSAHelper(rsaKey2.Text);
            //rSAHelper.EncryptionPadding = System.Security.Cryptography.RSAEncryptionPadding.OaepSHA1;
            var btArray= Convert.FromBase64String(richTextBox2.Text);
            var result = rSAHelper.Decrypt(btArray);
            richTextBox2.Text = Encoding.Default.GetString(result);
        }
    }
}
