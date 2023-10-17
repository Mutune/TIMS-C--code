using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Net;
using System.Reflection;
using TremolZFP;
using QRCoder;
using System.Text.RegularExpressions;

namespace SampleApp
{
    public partial class Form1 : Form
    {
        //**************************
        //Tremol Test Application 
        // Developed  by Jonathan Mutune Jnr
        // Developer - Mutune Jnr. Mutunejnr030@gmail.com
        //**************************

        private DataSet dsItems;
        private DataTable items;
        public TremolZFP.FP fp;
        string retval;
        int x;
        string address = "http://LocalHost:4444/";

        public Form1()
        {
            InitializeComponent();
            //Creating new instance of class FP
            //Property "ServerAddress" - Get/Set the server web address
            fp = new FP() { ServerAddress = address };

            //Everytime when the form is closed, ServerCloseDeviceConnection() must be called to close connection to the device.
            //AddHandler Closin, address
            //    closing += addresso;
            //AddHandler Closing, AddressOf OnClosing
        }
        private DataTable CreateItemsTable()
        {
            items = new DataTable("Items");
            // adding columns
            // AddNewColumn(items, "System.Boolean", "ColSelect")
            AddNewColumn(items, "System.Int32", "ID");
            AddNewColumn(items, "System.String", "Name");
            AddNewColumn(items, "System.String", "VATGroup");
            AddNewColumn(items, "System.String", "VATRate");
            AddNewColumn(items, "System.String", "Measure");
            AddNewColumn(items, "System.String", "HSCode");
            AddNewColumn(items, "System.String", "HSDesc");
            AddNewColumn(items, "System.String", "Sell");
            AddNewColumn(items, "System.String", "QTY");

            // adding rows
            AddNewRow(items, 1, "BREAD", "A", 16, "GRAMS", "", "", 50, 1);
            AddNewRow(items, 2, "PENCIL", "A", 16, "PC", "", "", 60, 1);
            AddNewRow(items, 3, "Spirit type jet fuel", "B", 8, "PC", "0105.13.00", "Spirit type jet fuel", 50, 1);
            AddNewRow(items, 4, "The supply of goods", "C", 0, "LTR", "0144.32.00", "The supply of goods", 50, 1);
            AddNewRow(items, 5, "Bovine Semen", "E", 0, "KG", "0001.11.00", "Bovine Semen", 20, 1);
            return items;
        }

        private void AddNewColumn(DataTable table, string columnType, string columnName)
        {
            DataColumn column = table.Columns.Add(columnName, Type.GetType(columnType));
        }
        private void AddNewRow(DataTable table, int id, string name, string VatGroup, int VatRate, string Measure, string HSCode, string HsDesc, double Sell, double Qty)
        {
            DataRow newrow = table.NewRow();
            // newrow("ColSelect") = False

            newrow["ID"] = items.Rows.Count + 1;
            newrow["Name"] = name;
            newrow["VATGroup"] = VatGroup;
            newrow["VATRate"] = VatRate;
            newrow["Measure"] = Measure;
            newrow["HSCode"] = HSCode;
            newrow["HSDesc"] = HsDesc;
            newrow["Sell"] = Sell;
            newrow["QTY"] = Qty;
            table.Rows.Add(newrow);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dsItems = new DataSet();
            dsItems.Tables.Add(CreateItemsTable());
            dataGridView1.DataSource = dsItems.Tables[0];
            PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                fp.ServerCloseDeviceConnection();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        private void HandleException(Exception x)
        {
            if (x is SException)
            {
                var sx = (SException)x;

                // Here are most iportant cases of errors
                switch (sx.ErrType)
                {
                    case SErrorType.ServerDefsMismatch:
                        // The current library version and server definitions version do not match
                        showMsg(sx); break;
                    case SErrorType.ServMismatchBetweenDefinitionAndFPResult:
                        // The current library version and the fiscal device firmware is not matching
                        showMsg(sx); break;
                    case SErrorType.ServerAddressNotSet:
                        // Specify server ServerAddress property 
                        showMsg(sx); break;
                    case SErrorType.ServerConnectionError:
                        // Connection from this app to the server is not established
                        showMsg(sx); break;
                    case SErrorType.ServSockConnectionFailed:
                        // When the server can not connect to the fiscal device
                        showMsg(sx); break;
                    case SErrorType.ServTCPAuth:
                        // Wrong device ТCP password
                        showMsg(sx); break;
                    case SErrorType.ServWaitOtherClientCmdProcessingTimeOut:
                        // Proccessing of other clients command is taking too long
                        showMsg(sx); break;

                    case SErrorType.FPException:
                        // Fiscal device is angry about your last command :)
                        /**
                        Posible reasons:  
                 sx.STE1 =                                              sx.STE2 =
                         30 OK                                                   30 OK                                 
                         31 Out of paper, printer failure                        31 Invalid command
                         32 Registers overflow                                   32 Illegal command
                         33 Clock failure or incorrect date&time                 33 Z daily report is not zero
                         34 Opened fiscal receipt                                34 Syntax error
                         35 Payment residue account                              35 Input registers overflow
                         36 Opened non-fiscal receipt                            36 Zero input registers
                         37 Registered payment but receipt is not closed         37 Unavailable transaction for correction
                         38 Fiscal memory failure                                38 Insufficient amount on hand
                         39 Incorrect password                                   3A No access
                         3a Missing external display
                         3b 24hours block – missing Z report
                         3c Overheated printer thermal head.
                         3d Interrupt power supply in fiscal receipt (one time until status is read)
                         3e Overflow EJ
                         3f Insufficient conditions
                      **/

                        if (sx.STE1 == 0x30 && sx.STE2 == 0x32)
                        {
                            //sx.STE1 == 0x30 - command is OK  AND  sx.STE2 == 0x32 - command is Illegal in current context    
                        }

                        if (sx.STE1 == 0x30 && sx.STE2 == 0x33)
                        {
                            //sx.STE1 == 0x30 - command is OK  AND sx.STE2 == 0x33 - make Z report
                        }

                        if (sx.STE1 == 0x34 && sx.STE2 == 0x32)
                        {
                            //sx.STE1 == 0x34 - Opened fiscal receipt  AND  sx.STE2 == 0x32 - command is Illegal in current context
                        }

                        showMsg(sx); break;
                    default: showMsg(sx); break;
                }
            }
            else
            {
                MessageBox.Show(x.ToString());
            }
        }
        private void showMsg(SException sx)
        {
            MessageBox.Show(sx.Message, Text, MessageBoxButtons.OK);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                fp.ServerCloseDeviceConnection();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (Button1.Text == "Disconnect")
                {
                    fp.ServerCloseDeviceConnection();
                    Button1.Text = "Connect TCP";
                }
                PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                retval = fp.ServerSetDeviceTcpSettings(txtIP.Text, int.Parse(txtPort.Text), "Password");
                var status = fp.ReadStatus();
                string TempserialNo = fp.ReadCUnumbers().SerialNumber;
                lblSerial.Text = TempserialNo;
                if (this.groupBox1.Controls.Count > 0)
                    this.groupBox1.Controls.Clear();
                int num1 = 0;
                int x = 20;
                int num2 = 15;
                Color color1 = Color.FromArgb(21, 123, 39);
                Color color2 = Color.FromArgb((int)byte.MaxValue, (int)sbyte.MaxValue, 39);
                PropertyInfo[] properties = new StatusRes().GetType().GetProperties();

                int index = ((IEnumerable<PropertyInfo>)properties).Count<PropertyInfo>() / 2 - 1;
                foreach (PropertyInfo propertyInfo in properties)
                {
                    bool flag = (bool)status.GetType().GetProperty(propertyInfo.Name).GetValue((object)status, (object[])null);
                    Label label = new Label();
                    label.Text = propertyInfo.Name.Replace('_', ' ');
                    label.Font = new Font("Microsoft Sans Serif", 7f);
                    label.AutoSize = true;
                    label.Location = new Point(x, num2 + num1);
                    this.groupBox1.Controls.Add((Control)label);
                    PictureBox pictureBox = new PictureBox();
                    pictureBox.BackColor = flag ? color2 : color1;
                    pictureBox.Location = new Point(x - 12, num2 + num1);
                    pictureBox.Size = new Size(12, 12);
                    this.groupBox1.Controls.Add((Control)pictureBox);
                    num1 += 15;
                    if (properties[index] == propertyInfo)
                    {
                        x = 210;
                        num1 = 0;
                    }
                }
                Button1.Text = "Disconnect";
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void btnSaveTran_Click()
        {
            try
            {
                txtInvNo.Text = Regex.Replace(txtInvNo.Text, "[^0-9a-zA-Z ]+", "");
                txtCustomer.Text = Regex.Replace(txtCustomer.Text, "[^0-9a-zA-Z ]+", "");

                /// Credit note only
                // '
                if (rbCredit.Checked == true)
                    // Credit notes
                    fp.OpenCreditNoteWithFreeCustomerData(txtCustomer.Text, txtPIN.Text, txtHead.Text, txtAdd.Text, txtPost.Text, txtExempt.Text, txtRIN.Text, txtInvNo.Text);
                else if (rbInvoice.Checked == true)
                    fp.OpenInvoiceWithFreeCustomerData(txtCustomer.Text, txtPIN.Text, txtHead.Text, txtAdd.Text, txtPost.Text, txtExempt.Text, txtInvNo.Text);
                else if (rbDebit.Checked == true)
                    fp.OpenDebitNoteWithFreeCustomerData(txtCustomer.Text, txtPIN.Text, txtHead.Text, txtAdd.Text, txtPost.Text, txtExempt.Text, txtRIN.Text, txtInvNo.Text);
                else if (rbReceipt.Checked == true)
                    fp.OpenReceipt(OptionReceiptFormat.Brief, txtInvNo.Text);
            }
            catch (SException ex1)
            {
                //Cancelling the receipt // Please check your workflow before cancelling -
                fp.CancelReceipt();
                MessageBox.Show("Transaction cancelled, please check the data missing...! STE1-" + ex1.STE1.ToString() + " STE2-" + ex1.STE2.ToString(), "POS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            OptionVATClass optionVAT;
            optionVAT = OptionVATClass.VAT_Class_A;
            for (var i = 0; i <= dataGridView1.RowCount - 1; i++)
            {
                var discAddV = "";
                decimal discAddP = default(Decimal);
                string itemname;
                decimal price, qty;
                if (discAddP == default(Decimal))
                    discAddP = 0;
                string measure, hscode, hsname;
                hsname = "";
                hscode = "";
                measure = "";
                decimal VatPerc;

                DataGridViewRow row = this.dataGridView1.Rows[i];
                if (!row.IsNewRow)
                {

                if (!String.IsNullOrEmpty(row.Cells["Name"].Value.ToString()))
                {
                    itemname = row.Cells["Name"].Value.ToString();
                    Regex.Replace(itemname, "[^0-9a-zA-Z ]+", "");
                    if (itemname.Length > 30)
                        itemname = itemname.Substring(0, 29);
                }
                else
                {
                    MessageBox.Show("No Item name");
                    return;
                }

                if (!String.IsNullOrEmpty(row.Cells["Sell"].Value.ToString()))
                {
                    price = decimal.Parse(row.Cells["Sell"].Value.ToString());
                }
                else
                {
                    MessageBox.Show("No sell price");
                    return;
                }

                if (!String.IsNullOrEmpty(row.Cells["QTY"].Value.ToString()))
                    qty = decimal.Parse(row.Cells["QTY"].Value.ToString());
                else
                    qty = 0;

                if (!String.IsNullOrEmpty(row.Cells["HSCODE"].Value.ToString()))
                    hscode = row.Cells["HSCODE"].Value.ToString();
                else
                    hscode = "";

                if (!String.IsNullOrEmpty(row.Cells["HSDESC"].Value.ToString()))
                {
                    hsname = row.Cells["HSDESC"].Value.ToString();
                    if (hsname.Length > 20)
                        hsname = hsname.Substring(0, 19);
                }
                else
                    hsname = "";
                string Vatname;
                if (!String.IsNullOrEmpty(row.Cells["VATGroup"].Value.ToString())) {
                    Vatname = row.Cells["VATGroup"].Value.ToString();
                    if (Vatname == "A")
                    {
                        optionVAT = OptionVATClass.VAT_Class_A;
                    }
                    else if (Vatname == "B")
                    {
                        optionVAT = OptionVATClass.VAT_Class_B;
                    }
                    else if (Vatname == "C")
                    {
                        optionVAT = OptionVATClass.VAT_Class_C;
                    }
                    else if (Vatname == "D")
                    {
                        optionVAT = OptionVATClass.VAT_Class_D;
                    }
                    else if (Vatname == "E")
                    {
                        optionVAT = OptionVATClass.VAT_Class_E;
                    }
                }
                else
                {
                    MessageBox.Show("No Vat specified");
                    return;
                }

                if (!String.IsNullOrEmpty(row.Cells["VATRate"].Value.ToString()))
                    VatPerc = decimal.Parse(row.Cells["VATRate"].Value.ToString());
                else
                {
                    MessageBox.Show("No Vat specified");
                    return;
                }

                    if (!String.IsNullOrEmpty(row.Cells["Measure"].Value.ToString()))
                    {
                        measure = row.Cells["Measure"].Value.ToString();
                        measure = Regex.Replace(measure, "[^0-9a-zA-Z ]+", "");
                    }
                    else
                        measure = "";
                try
                    {
                        string TestVatPer = "";
                        TestVatPer = String.Format(VatPerc.ToString(), "00.00");
                        fp.SellPLUfromExtDB(itemname, optionVAT, price, measure, hscode, hsname, decimal.Parse(TestVatPer), qty, discAddP);
                    }
                    catch (SException ex)
                    {
                        //Cancelling the receip // Please check your workflow before cancelling -
                        fp.CancelReceipt();
                        MessageBox.Show("Receipt cancelled, please try again...! STE1-" + ex.STE1.ToString() + " STE2-" + ex.STE2.ToString(), "POS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            CurrentReceiptInfoRes rcpinfo = new CurrentReceiptInfoRes();
                rcpinfo = fp.ReadCurrentReceiptInfo();
                lblVatA.Text = "Subtotal A " + rcpinfo.SubtotalAmountVATGA.ToString();
                lblVatB.Text = "Subtotal B " + rcpinfo.SubtotalAmountVATGB.ToString();
                lblVatC.Text = "Subtotal C " + rcpinfo.SubtotalAmountVATGC.ToString();
                lblVatD.Text = "Subtotal D " + rcpinfo.SubtotalAmountVATGD.ToString();
                lblVatE.Text = "Subtotal E " + rcpinfo.SubtotalAmountVATGE.ToString();

                CloseReceiptRes closeReceiptRes = new CloseReceiptRes();
                closeReceiptRes = fp.CloseReceipt();
                string tempQr = closeReceiptRes.QRcode;
                txtCUInv.Text = closeReceiptRes.InvoiceNum;
                txtQr.Text = tempQr;
                QRCoder.QRCodeGenerator qrn = new QRCoder.QRCodeGenerator();
                var data = qrn.CreateQrCode(txtQr.Text, QRCoder.QRCodeGenerator.ECCLevel.Q);
                QRCoder.QRCode code = new QRCoder.QRCode(data);
                PictureBox1.Image = code.GetGraphic(6);
                PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                lblDatetime.Text = fp.ReadDateTime().ToString();
                
            }

        private void btnSaveTran_Click_1(object sender, EventArgs e)
        {
            btnSaveTran_Click(); 
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            chkDhcp.Checked = false;
            if (fp.ReadDHCP_Status()==OptionDHCPEnabled.Enabled)
            {
                chkDhcp.Checked = true;
            }
            txtSetIP.Text = fp.ReadDeviceTCP_Addresses(OptionAddressType.IP_address).DeviceAddress;
            txtSub.Text = fp.ReadDeviceTCP_Addresses(OptionAddressType.Subnet_Mask).DeviceAddress;
            txtGate.Text = fp.ReadDeviceTCP_Addresses(OptionAddressType.Gateway_address).DeviceAddress;
            txtDNs.Text = fp.ReadDeviceTCP_Addresses(OptionAddressType.DNS_address).DeviceAddress;
            fp.SetTCP_AutoStart(fp.ReadTCP_AutoStartStatus());
            fp.SetTCP_ActiveModule(OptionUsedModule.LAN_module);
        }

        private void btnSetTCP_Click(object sender, EventArgs e)
        {
            if (chkDhcp.Checked)
            {
                fp.SetDHCP_Enabled(OptionDHCPEnabled.Enabled);
            }
            else
           {
                fp.SetDHCP_Enabled(OptionDHCPEnabled.Disabled);
            }
            fp.SetDeviceTCP_Addresses(OptionAddressType.IP_address, txtSetIP.Text);
            fp.SetDeviceTCP_Addresses(OptionAddressType.Subnet_Mask, txtSub.Text);
            fp.SetDeviceTCP_Addresses(OptionAddressType.Gateway_address, txtGate.Text);
            fp.SetDeviceTCP_Addresses(OptionAddressType.DNS_address, txtDNs.Text);
            fp.SetTCP_AutoStart(fp.ReadTCP_AutoStartStatus());
            fp.SetTCP_ActiveModule(OptionUsedModule.LAN_module);
            
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            fp.ServerCloseDeviceConnection();
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            DailyAmountsByVATRes dailyAmounts = fp.ReadDailyAmountsByVAT();
            MessageBox.Show(dailyAmounts.SaleAmountVATGrA.ToString());
            MessageBox.Show(dailyAmounts.SaleAmountVATGrB.ToString());
            MessageBox.Show(dailyAmounts.SaleAmountVATGrC.ToString());
            MessageBox.Show(dailyAmounts.SaleAmountVATGrD.ToString());
            MessageBox.Show(dailyAmounts.SaleAmountVATGrE.ToString());
            MessageBox.Show(dailyAmounts.RefundAmountVATGrA.ToString() + "REf");
            MessageBox.Show(dailyAmounts.RefundAmountVATGrB.ToString());
            MessageBox.Show(dailyAmounts.RefundAmountVATGrC.ToString());
            MessageBox.Show(dailyAmounts.RefundAmountVATGrD.ToString());
            MessageBox.Show(dailyAmounts.RefundAmountVATGrE.ToString());
            MessageBox.Show(dailyAmounts.TurnoverAmountVAT.ToString());
            MessageBox.Show(dailyAmounts.TurnoverRefAmountVAT.ToString());

        }

        //**************************
        //Tremol Test Application 
        // Developed  by Your Apps Ltd
        // Developer - DJ - 0739554444 0734665555
        //**************************

    }
    }

