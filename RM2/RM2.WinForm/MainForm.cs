
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
using RM2.Framework.AopServe;

namespace RM2.WinForm
{
    public partial class MainForm : Form
    {
        private IUserService UserService = null;
        private ILogService LogService = null;
        private IRoleService RoleService = null;
        private IMenuService MenuService = null;
        public MainForm()
        {
            InitializeComponent();
            UserService = AopExtend.Container().Resolve<IUserService>();
            LogService = AopExtend.Container().Resolve<ILogService>();
            RoleService = AopExtend.Container().Resolve<IRoleService>();
            MenuService = AopExtend.Container().Resolve<IMenuService>();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //插入用户 代码
            UserService.AddUser(new Model.Base_User { CreateDate=DateTime.Now});
           
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
