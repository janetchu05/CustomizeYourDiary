using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using static System.Windows.Forms.ListViewItem;

namespace CustomizeYourDiary
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        //마우스 왼클릭으로 borderless 폼 drag (START)
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        //--마우스 왼클릭으로 borderless 폼 drag (END)


        //리스트뷰 선택하면 우측 그룹박스에 정보 표시
        private void lvResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbTheme.Enabled = true;
            cbPage.Enabled = true;
            if(lvResult.SelectedItems.Count > 0)
            {
                ListViewItem lvSelected = lvResult.SelectedItems[0];
                lblSelectedContents.Text = lvSelected.SubItems[1].Text;
                cbTheme.Text = lvSelected.SubItems[2].Text;
                cbPage.Text = lvSelected.SubItems[3].Text;
            }
        }

        //우측 그룹박스에서 테마 변경하면 리스트뷰에서도 변경 + 미리보기 테마 변경
        private void cbTheme_SelectedValueChanged(object sender, EventArgs e)
        {
            ListViewItem lvSelected = lvResult.SelectedItems[0];
            lvSelected.SubItems[2].Text = cbTheme.Text;

            CreatePreview();
        }

        //우측 그룹박스에서 페이지 수 변경하면 리스트뷰에서도 변경
        private void cbPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewItem lvSelected = lvResult.SelectedItems[0];
            lvSelected.SubItems[3].Text = cbPage.Text;
        }

        //클릭시 리스트뷰 전체 테마 변경
        private void btnApplyAll_Click(object sender, EventArgs e)
        {
            ListViewItem lvSelected = lvResult.SelectedItems[0];
            for (int i = 0; i < lvResult.Items.Count; i++)
            {
                lvResult.Items[i].SubItems[2].Text = lvSelected.SubItems[2].Text;
            }
        }

        //리스트뷰 row를 전달인자로 주고, 그 row의 내용에 따라 소스 path 생성
        private string SelectImage(ListViewItem itemRow)
        {
            //이미지 파일 설정
            string curTheme = itemRow.SubItems[2].Text;
            string Selected;

            switch (curTheme)
            {
                case "우주":
                    Selected = itemRow.SubItems[1].Text + "Universe.jpg";
                    break;

                case "꽃":
                    Selected = itemRow.SubItems[1].Text + "Flower.jpg";
                    break;

                case "컬러풀":
                    Selected = itemRow.SubItems[1].Text + "Color.jpg";
                    break;

                default:
                    Selected = "Universe.jpg";      //임시로 해놓은 이미지
                    break;
            }
            return Selected;
        }

        //리스트 뷰 각 row 내용에 따라 preview 화면 변경
        private void CreatePreview()
        {
            if(lvResult.SelectedItems.Count != 0)
            {
                string bgSrc = SelectImage(lvResult.SelectedItems[0]);

                pbPreview.Load(bgSrc);
                pbPreview.SizeMode = PictureBoxSizeMode.Zoom;
            }
            

        }

        /*생성버튼 클릭시 정보란 비어있는지 확인
         정보 모두 쓰여있으면 "이름_번호"로 파일 이름 string 생성
        pdf document 생성하고 foreach로 listview의 각 row 읽으면서 addpage
        각 페이지에는 SelectImage 함수로 소스 path 지정한 jpg 이미지 그리기*/
        private void btnMake_Click(object sender, EventArgs e)
        {
            if (txtName.Text == "" && txtPhone.Text == "")
            {
                MessageBox.Show("정보를 입력해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
            }
            else if (txtName.Text == "")
            {
                MessageBox.Show("이름을 입력해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
            }
            else if(txtPhone.Text == "")
            {
                MessageBox.Show("전화번호를 입력해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
            }
            else
            {
                try
                {
                    string FileName = txtName.Text + "_" + txtPhone.Text + ".pdf";
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = FileName;

                    foreach(ListViewItem items in lvResult.Items)
                    {
                        if(items.Checked)
                        {
                            int pageNumb = int.Parse(items.SubItems[3].Text);
                            string source = SelectImage(items);
                            
                            for (int i = 0; i < pageNumb; i++)
                            {
                                PdfPage page = document.AddPage();
                                XGraphics gfx = XGraphics.FromPdfPage(page);
                                XImage img = XImage.FromFile(source);
                                gfx.DrawImage(img, 0, 0, page.Width, page.Height);
                            }
                        }
                    }

                    document.Save(FileName);
                    Process.Start(FileName);
                    MessageBox.Show("PDF가 생성되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "알림", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }
    }
}
