using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.borgle.qqreader.Model
{
    public class QQDatabase : GalaSoft.MvvmLight.ViewModelBase
    {
        private String _IMEI;
        public String IMEI
        {
            get { return _IMEI; }
            set
            {
                Set<String>(ref _IMEI, value);
            }
        }
        private String _FilePath;
        public String FilePath
        {
            get { return _FilePath; }
            set
            {
                Set<String>(ref _FilePath, value);
            }
        }
    }
}
