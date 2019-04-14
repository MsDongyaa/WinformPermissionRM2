
using RM2.Framework.IceAop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Practices.Unity;
using Unity;
using RM2.IService;

namespace RM2.WinForm
{
    public partial class MainForm : Form
    {
        private IUserService UserService = null;
        public MainForm()
        {
            InitializeComponent();
            UserService = AopExtend.Container().Resolve<IUserService>();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //插入用户 代码
            UserService.AddUser(new Model.Base_User { CreateDate=DateTime.Now});
        }
    }
}
