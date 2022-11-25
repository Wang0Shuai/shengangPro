using GemBox.Spreadsheet;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K3Cloud.ShenGang.BLL.Service.Plugin
{
    
    [HotUpdate]
    public class ExcelHelper
    {
        public static DataSet Import(string fileName, string colName, int Line)
        {
            DataSet dataSet = new DataSet();
            SpreadsheetInfo.SetLicense("E0YU-SGRA-DYIO-M01S");
            ExcelFile ef = new ExcelFile();
            switch (Path.GetExtension(fileName).ToUpper())
            {
                case ".XLS":
                    ef.LoadXls(fileName);
                    break;
                case ".XLSX":
                    ef.LoadXlsx(fileName, XlsxOptions.None);
                    break;
                case ".CSV":
                    ef.LoadCsv(fileName, CsvType.TabDelimited);
                    break;
            }

            bool flag = false;
            foreach (var sheet in ef.Worksheets)
            {
                if (sheet.Name.Equals("委外订单"))
                {


                    DataTable table = new DataTable();
                    table.TableName = sheet.Name;
                    int columns = sheet.CalculateMaxUsedColumns();
                    int colNameIndex = Line;//由于excel文件需要指定从第几行开始当做列名被读取返回
                    for (int i = 0; i < columns; i++)
                    {
                        var cell = sheet.Cells[colNameIndex, i];
                        string cellValue = cell.Value == null ? string.Empty : cell.Value.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(cellValue) && !table.Columns.Contains(cellValue))
                        {
                            table.Columns.Add(cellValue);
                        }
                        else
                        {
                            throw new Exception("列名重复或为空");
                        }
                    }
                    if (table.Columns.Count < 1)
                    {
                        continue;
                    }
                    for (int i = colNameIndex + 1; i < sheet.Rows.Count; i++)
                    {
                        if (sheet.Cells[i, 0].Value == null)
                        {
                            continue;
                        }
                        DataRow row = table.NewRow();
                        for (int j = 0; j < table.Columns.Count; j++)
                        {
                            row[j] = sheet.Cells[i, j].Value == null ? string.Empty : sheet.Cells[i, j].Value.ToString().Trim();
                        }
                        table.Rows.Add(row);
                    }

                    dataSet.Tables.Add(table);
                }
            }
            return dataSet;
        }
        //列转行
        private DataTable PivotDatatable(DataTable dtSource, string columnFilter)
        {
            var columns = columnFilter.Split(',');
            DataTable dtFilter = dtSource.DefaultView.ToTable(false, columns);
            DataTable dtResult = new DataTable();

            var rowCount = dtFilter.Rows.Count;
            var columnCount = columns.Length;

            // 源数组的行数比DataTable的行数+1,, 加一行表头
            object[,] arrSource = new object[rowCount + 1, columnCount];

            // 目标数组的行数等于选择的列数,列数等于 源数据的行数+1, 加一列 属性名
            object[,] arrResult = new object[columnCount, rowCount + 1];

            // 原数组第一行写表头
            for (int i = 0; i < columnCount; i++)
            {
                arrSource[0, i] = dtFilter.Columns[i].ColumnName;
            }

            // 源数据 每一行写 数据
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    arrSource[i + 1, j] = dtFilter.Rows[i][j];
                }
            }

            // 原数 转置到 目标数组
            for (int i = 0; i < rowCount + 1; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    arrResult[j, i] = arrSource[i, j];
                }
            }

            // 创建 Datatable 的结构
            for (int i = 0; i < rowCount + 1; i++)
            {
                dtResult.Columns.Add(arrResult[0, i].ToString());
            }

            List<string> valueList = new List<string>();
            for (int i = 1; i < columnCount; i++)
            {
                for (int j = 0; j < rowCount + 1; j++)
                {
                    valueList.Add(arrResult[i, j].ToString());
                }

                dtResult.Rows.Add(valueList.ToArray());
                valueList.Clear();
            }
            return dtResult;
        }
        //行转列
        private DataTable SwapTable(DataTable tableData)
        {
            int intRows = tableData.Rows.Count;
            int intColumns = tableData.Columns.Count;

            //转二维数组
            string[,] arrayData = new string[intRows, intColumns];
            for (int i = 0; i < intRows; i++)
            {
                for (int j = 0; j < intColumns; j++)
                {
                    arrayData[i, j] = tableData.Rows[i][j].ToString();
                }
            }
            //下标对换
            string[,] arrSwap = new string[intColumns, intRows];
            for (int m = 0; m < intColumns; m++)
            {
                for (int n = 0; n < intRows; n++)
                {
                    arrSwap[m, n] = arrayData[n, m];
                }
            }
            DataTable dt = new DataTable();
            //添加列
            for (int k = 0; k < intRows; k++)
            {
                dt.Columns.Add(
                        new DataColumn(arrSwap[0, k])
                    );
            }
            //添加行
            for (int r = 1; r < intColumns; r++)
            {
                DataRow dr = dt.NewRow();
                for (int c = 0; c < intRows; c++)
                {
                    dr[c] = arrSwap[r, c].ToString();
                }
                dt.Rows.Add(dr);
            }
            //添加行头
            DataColumn ColRowHead = new DataColumn(tableData.Columns[0].ColumnName);
            dt.Columns.Add(ColRowHead);
            dt.Columns[ColRowHead.ColumnName].SetOrdinal(0);
            for (int i = 0; i < intColumns - 1; i++)
            {
                dt.Rows[i][ColRowHead.ColumnName] = tableData.Columns[i + 1].ColumnName;
            }
            return dt;
        }

        //动态列转行
        public static DataTable ConvTable(DataTable dt, string[] aggfrom, string aggto, string val)
        {
            DataTable dt1 = new DataTable();
            foreach (DataColumn col in dt.Columns)
            {
                if (!aggfrom.Contains(col.ColumnName))
                    dt1.Columns.Add(col.ColumnName);
            }
            dt1.Columns.Add(aggto, typeof(DateTime));
            dt1.Columns.Add(val);
            var query = dt.Rows.Cast<DataRow>()
                .SelectMany(x => aggfrom.Select(y =>
                {
                    var row = dt1.NewRow();
                    foreach (DataColumn col in dt1.Columns)
                    {
                        if (col.ColumnName == aggto)
                        {
                            row[col.ColumnName] = y;
                        }
                        else if (col.ColumnName == val)
                        {
                            row[col.ColumnName] = x[y];
                        }
                        else
                        {
                            row[col.ColumnName] = x[col.ColumnName];
                        }
                    }
                    return row;
                }));
            foreach (var r in query)
                dt1.Rows.Add(r);
            return dt1;
        }

    }
}
