using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyWpfForismatic
{
    /// <summary>
    /// Interaction logic for frmThoughts.xaml
    /// </summary>
    public partial class frmThoughts : Window
    {
        ///// <summary> NextId == Thoughts.Id at SqlServer </summary>
        //public int NextId = -1;
        /// <summary> NextAuthorId == Thoughts.AuthorId at SqlServer </summary>
        public int NextAuthorId = -1;



        public frmThoughts()
        {
            InitializeComponent();
            this.Loaded += FrmThoughts_Loaded;
        }

        #region FrmThoughts_Loaded()
        private void FrmThoughts_Loaded(object sender, RoutedEventArgs e)
        {
            bool is_item_selected = false;

            TreeViewItem parent = new TreeViewItem();
            parent.Header = "Авторы";
            parent.Tag = 0;  // ==>   PatternInfo(1, "Фабричный метод", "Factory Method", EnumPatternType.Creational, "pics/factorymethod.png");
            // parent.IsExpanded = true;
            treeAuthors.Items.Add(parent);

            foreach (Authors cur_author in ThoughtsRepository.authors_list)
            {
                TreeViewItem child = new TreeViewItem();
                child.Header = cur_author.AuthorName; // + " (" + cur_author.ThoughtsCount + ")";

                //++ second row - for Eng name of pattern
                child.ToolTip = " (" + cur_author.ThoughtsCount + ")";

                // child.IsExpanded = true;
                child.Tag = cur_author.Id;
                parent.Items.Add(child);

                // selcting item with author_id == 1
                //
                if (NextAuthorId > 0)
                {
                    if (cur_author.Id == NextAuthorId)
                    {
                        child.IsSelected = true;
                        is_item_selected = true;

                        // scroll to this item
                        // treeAuthors.BringIntoView()
                        child.BringIntoView();
                    }
                }
                else
                {
                    if (cur_author.Id == 1)
                    {
                        child.IsSelected = true;
                        is_item_selected = true;
                    }
                }
            }
            // expand root
            //
            parent.IsExpanded = true;

            // select root at the tree
            //
            if (!is_item_selected)
                parent.IsSelected = true;

            // scroll tree to selected item
            //
            //Node31.IsSelected = true;
            //treeview.BringIntoView(treeview.SelectedItem as TreeViewItemAdv);

            //ScrollViewer scrollviewer = treeview.Template.FindName("PART_ScrollViwer", treeview) as ScrollViewer;

            //Povide the required vertical offset to scroll
            // scrollviewer.ScrollToVerticalOffset(50);
        }
        #endregion FrmThoughts_Loaded()

        #region treeAuthors_SelectedItemChanged()
        private void treeAuthors_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            string msg = "";
            int id = -1;
            try
            {
                TreeView tv = (TreeView)sender;
                TreeViewItem tvItem = (TreeViewItem)tv.SelectedItem;

                string header = tvItem.Header.ToString(); //.ToLower();
                msg = header.Trim();
                if (tvItem.Tag != null)
                {
                    string s_tag = tvItem.Tag.ToString();
                    if (Int32.TryParse(s_tag, out id))
                    {
                        msg += " (id = " + id.ToString() + ")";
                    }
                    else
                    {
                        id = -1;
                        msg += " (" + s_tag + ")";
                    }
                }

                //TreeViewItem tvParent = null;
                //string header_parent = "";
                //if (tvItem != null)
                //{
                //    tvParent = tvItem.Parent as TreeViewItem;
                //    if (tvParent != null)
                //    {
                //        header_parent = tvParent.Header.ToString().ToLower();
                //        msg += " [" + header_parent + "]";
                //    }
                //}

                //
                //--------  Показываем / прячем нужный контейнер - для паттернов, SOLID,...       ---------------->>>
                //
                if (id >= 0) // && id < 1000)
                {
                    // stackCommonDefs
                    //
                    Authors cur_author = new Authors(id, header);
                    stackCommonDefs.DataContext = cur_author;

                    // lvCommonDefs
                    //
                    lvCommonDefs.ItemsSource = ThoughtsRepository.GetThoughtsByAuthorId(id);
                }
                else
                {
                    // stackCommonDefs
                    //
                    Authors cur_author = null; //  new Authors(id, header);
                    stackCommonDefs.DataContext = cur_author;

                    // lvCommonDefs
                    //
                    lvCommonDefs.ItemsSource = null;    // ThoughtsRepository.GetThoughtsByAuthorId(id);
                }

            }
            catch (Exception x)
            {
                msg = "ERROR: " + x.Message;
            }

            // show selected tree view item
            //
            textCurrentTreeItem.Text = msg;
            textCurrentTreeItem.Tag = id;

            // reset info for selected expander
            textCurrentExpander.Text = ""; // "Selected Expander = " + data.NameRus;
        }
        #endregion treeAuthors_SelectedItemChanged()

        #region ButtonDeleteClick()
        private void ButtonDeleteClick(object sender, RoutedEventArgs e)
        {
            Button cur_but = (Button)sender;
            if (cur_but != null)
            {
                int author_id = -1;
                int id = -1;

                if (int.TryParse(textCurrentTreeItem.Tag.ToString(), out author_id) && int.TryParse(cur_but.Tag.ToString(), out id))
                {
                    if (MessageBox.Show("Удалить эту мысль\r\n(id = " + id.ToString() +
                        "),\r\nдля " + textCurrentTreeItem.Text + "\r\n???", "Deleting thought",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        bool ret_val = ThoughtsRepository.DeleteRecordFromSqlServer(id);
                        Console.WriteLine("--- var result = ThoughtsRepository.DeleteRecordFromSqlServer({0}) ---", id);

                        // Reload Data from SqlServer
                        ThoughtsRepository.LoadDataFromServer();
                        Console.WriteLine("--- Reload Data from SqlServer ---");

                        // ItemsSource for lvCommonDefs - reload
                        //
                        lvCommonDefs.ItemsSource = null;
                        lvCommonDefs.ItemsSource = ThoughtsRepository.GetThoughtsByAuthorId(author_id);
                    }
                }
            }
        }
        #endregion ButtonDeleteClick()

        #region butClose_Click()
        private void butClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion butClose_Click()

        #region Browse Current Thought at IE
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            //
            // try open through 'explorer.exe'
            try
            {
                // Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));

                string p_url = e.Uri.AbsoluteUri; // "http://metanit.com/sharp/patterns/1.2.php";
                ProcessStartInfo startInfo = new ProcessStartInfo("iexplore.exe", p_url);
                Process.Start(startInfo);
            }
            catch { }

            e.Handled = true;
        }
        #endregion Browse Current Thought at IE

    }

}
