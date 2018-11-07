using GalaSoft.MvvmLight.Command;
using io.borgle.core.ViewModel;
using io.borgle.Core.Helper;
using io.borgle.qqreader.Model;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Windows.Input;
using static io.borgle.Controls.Helper.DataGridColumnHelper;

namespace io.borgle.qqreader.ViewModel
{
    class MainWindowVM : ViewModelBase
    {
        private readonly IDialogCoordinator _dialogCoordinator = DialogCoordinator.Instance;
        private SqliteHelper dbHelper;

        public MainWindowVM()
        {
            this.DatabaseInfo = new QQDatabase();
            this.TableItems = new ObservableCollection<string>();
            this.TableDataItems = new ObservableCollection<List<Object>>();
            this.TableColumns = new ObservableCollection<ColumnInfo>();
        }

        private int _SelectedTabItemIndex;
        public int SelectedTabItemIndex
        {
            get { return _SelectedTabItemIndex; }
            set { Set<int>(ref _SelectedTabItemIndex, value); }
        }
        private QQDatabase _database;
        public QQDatabase DatabaseInfo
        {
            get { return _database; }
            set { Set<QQDatabase>(ref _database, value); }
        }
        private ObservableCollection<String> _tableitems;
        public ObservableCollection<String> TableItems
        {
            get { return _tableitems; }
            set { Set<ObservableCollection<String>>(ref _tableitems, value); }
        }
        private ObservableCollection<List<Object>> _tableDataItems;
        public ObservableCollection<List<Object>> TableDataItems
        {
            get { return _tableDataItems; }
            set { Set<ObservableCollection<List<Object>>>(ref _tableDataItems, value); }
        }

        private ObservableCollection<ColumnInfo> _tableColumns;
        public ObservableCollection<ColumnInfo> TableColumns
        {
            get { return _tableColumns; }
            set { Set<ObservableCollection<ColumnInfo>>(ref _tableColumns, value); }
        }
        #region 命令事件
        public ICommand SelectDbFileCommand
        {
            get
            {
                return new RelayCommand(SelectDbFileCommandWorker, () => true);
            }
        }
        public ICommand ReadDBCommand
        {
            get
            {
                return new RelayCommand(ReadDBCommandWorker, () => true);
            }
        }
        public ICommand TableSelectedCommand
        {
            get
            {
                return new RelayCommand<String>(TableSelectedCommandWorker, (tb) => true);
            }
        }
        #endregion

        private void SelectDbFileCommandWorker()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".db";
            dialog.Filter = "SQLite 文件 (.db)|*.db";

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                this.DatabaseInfo.FilePath = dialog.FileName;
            }
        }

        private void ReadDBCommandWorker()
        {
            if (String.IsNullOrEmpty(this.DatabaseInfo.FilePath))
            {
                _dialogCoordinator.ShowMessageAsync(this, "出错了", "请先选择一个 QQ 数据库文件");
                return;
            }
            String connectionString = string.Format("Data Source={0};Version=3", this.DatabaseInfo.FilePath);
            this.dbHelper = new SqliteHelper(connectionString);
            this.TableItems.Clear();
            this.QueryTables().ForEach(tb => {
                this.TableItems.Add(tb);
            });
            this.SelectedTabItemIndex = 1;
        }

        private void TableSelectedCommandWorker(String tableName)
        {
            String sql = "select * from " + tableName;
            SQLiteDataReader reader = dbHelper.ExecuteReader(sql);

            this.TableColumns.Clear();
            int fieldCount = reader.FieldCount;
            for (int i = 0; i < fieldCount; i++)
            {
                var columnInfo = new ColumnInfo
                {
                    HeaderText = reader.GetName(i),
                    DisplayMemberPath = String.Format("[{0}]", i),
                };
                this.TableColumns.Add(columnInfo);
            }

            this.TableDataItems.Clear();
            while (reader.Read())
            {
                List<Object> rowData = new List<Object>();
                for (int i = 0; i < fieldCount; i++)
                {
                    String data = null;
                    if (!reader.IsDBNull(i))
                    {
                        if (reader.GetDataTypeName(i) == "TEXT")
                        {
                            data = Decode(reader.GetString(i));
                        }
                        else
                        {
                            data = reader.GetValue(i).ToString();
                        }
                    }
                    rowData.Add(data);
                }
                this.TableDataItems.Add(rowData);
            }
            reader.Close();
        }


        private List<String> QueryTables()
        {
            String sql = "select name from sqlite_master where type='table' order by name";
            SQLiteDataReader reader = dbHelper.ExecuteReader(sql);
            List<String> tableNameList = new List<string>();
            while (reader.Read())
            {
                tableNameList.Add(reader.GetString(0));
            }
            reader.Close();
            return tableNameList;
        }

        private String Decode(String str)
        {
            if (str == null)
            {
                return null;
            }
            if (String.IsNullOrEmpty(this.DatabaseInfo.IMEI))
            {
                return str;
            }
            char[] codeKey = this.DatabaseInfo.secKey;
            int codeKeyLen = this.DatabaseInfo.secKeyLength;
            char[] input = str.ToCharArray();
            char[] output = new char[input.Length];
            int i;
            if (codeKeyLen >= input.Length)
            {
                for (i = 0; i < input.Length; i++)
                {
                    output[i] = (char)(input[i] ^ codeKey[i]);
                }
            }
            else
            {
                for (i = 0; i < input.Length; i++)
                {
                    output[i] = (char)(input[i] ^ codeKey[i % codeKeyLen]);
                }
            }
            if (output.Length == 0)
            {
                return "";
            }
            return new String(output);
        }
    }
}
