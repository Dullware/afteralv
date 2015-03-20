using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

public class ButtonOK : Button
{
    public void MyClick() {
        OnClick(new EventArgs());
    }
}

public class StartupScreen : System.Windows.Forms.Form
{
    private ButtonOK btOK;
    private System.Windows.Forms.Button btQUIT;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label result;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    delegate void SetResultTextDelegate(string s);
    delegate void SetOKButtonDelegate(bool s);

    public StartupScreen()
    {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();

        //
        // TODO: Add any constructor code after InitializeComponent call
        //
        //BackColor = Color.LightCyan;
        Thread verify = new Thread(new ThreadStart(doudp));
        verify.Start();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>

    public void SetResultText(string s)
    {
        if (!result.InvokeRequired)
        {
            result.Text = s;
        }
        else //We are on a non GUI thread.
        {
            SetResultTextDelegate ssbtDel = new SetResultTextDelegate(SetResultText);
            result.Invoke(ssbtDel, new object[] { s });
        }
    }

    public void SetOKButton(bool s)
    {
        if (!result.InvokeRequired)
        {
            btOK.Enabled = s;
            if (btOK.Enabled)
            {
                btOK.Select();
                btOK.MyClick();
            }
        }
        else //We are on a non GUI thread.
        {
            SetOKButtonDelegate ssbtDel = new SetOKButtonDelegate(SetOKButton);
            btOK.Invoke(ssbtDel, new object[] { s });
        }
    }

    void doudp()
    {
        udp udp = null;
        bool passed = false;
        bool ready = false;
        string[] hosts = { "82.95.178.145", "137.224.24.211" };
        foreach (string host in hosts)
        {
            if (udp != null) udp.Close();
            udp = new udp(host);
            if ( !udp.SendDatagram(Environment.UserName + "@" + System.Net.Dns.GetHostName() + "\t" + "AfterALV" + "\t14") )
            	continue;
            for (; ; )
            {
                string[] splittedansw;
                string answ = udp.GetDatagram();
                if (answ == null) break;
                splittedansw = answ.Split(new Char[] { '\t' });
                if (splittedansw.Length > 2 && splittedansw[1] == "AfterALV")
                {
                    ready = true;
                    SetResultText(splittedansw[2].Substring(1));
                    passed = splittedansw[2].StartsWith("+");
                    break;
                }
            }
            if (ready) break;
        }

        if (result.Text == "Please wait...") SetResultText("Failed (no server found).\r\nBe sure to let your firewall allow us to connect to \r\n82.95.178.145 using UDP port 7040.");
        SetOKButton(passed);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (components != null)
            {
                components.Dispose();
            }
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.btOK = new ButtonOK();
        this.btQUIT = new System.Windows.Forms.Button();
        this.label1 = new System.Windows.Forms.Label();
        this.result = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // btOK
        // 
        this.btOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.btOK.Enabled = false;
        this.btOK.Location = new System.Drawing.Point(336, 136);
        this.btOK.Name = "btOK";
        this.btOK.TabIndex = 0;
        this.btOK.Text = "OK";
        // 
        // btQUIT
        // 
        this.btQUIT.DialogResult = System.Windows.Forms.DialogResult.Abort;
        this.btQUIT.Location = new System.Drawing.Point(224, 136);
        this.btQUIT.Name = "btQUIT";
        this.btQUIT.TabIndex = 1;
        this.btQUIT.Text = "Quit";
        this.btQUIT.Click += new System.EventHandler(this.btQUIT_Click);
        // 
        // label1
        // 
        this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
        this.label1.Location = new System.Drawing.Point(8, 16);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(280, 48);
        this.label1.TabIndex = 2;
        this.label1.Text = "Verifying your copy of AfterALV:";
        // 
        // result
        // 
        this.result.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
        this.result.Location = new System.Drawing.Point(8, 64);
        this.result.Name = "result";
        this.result.Size = new System.Drawing.Size(424, 48);
        this.result.TabIndex = 3;
        this.result.Text = "Please wait...";
        // 
        // StartupScreen
        // 
        this.AcceptButton = this.btOK;
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.CancelButton = this.btQUIT;
        this.ClientSize = new System.Drawing.Size(426, 192);
        this.ControlBox = false;
        this.Controls.Add(this.result);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.btQUIT);
        this.Controls.Add(this.btOK);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "StartupScreen";
        this.Text = "StartupScreen";
        this.ResumeLayout(false);

    }
    #endregion

    private void btQUIT_Click(object sender, System.EventArgs e)
    {
    	try
    	{
        	if (!btOK.Enabled) System.Diagnostics.Process.Start("http://albus.fenk.wur.nl/~dullware");		
    	}
    	catch{}
    }

}
