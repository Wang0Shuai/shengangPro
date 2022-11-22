using Kingdee.BOS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K3Cloud.ShenGang.BLL.Service.Plugin
{
    [Kingdee.BOS.Util.HotUpdate]
    public class ExcelHelperTool
    {
        /// 导出为Excel格式文件
        /// </summary>
        /// <param name="dt">作为数据源的DataTable</param>
        /// <param name="saveFile">带路径的保存文件名</param>
        /// <param name="title">一个Excel sheet的标题</param>
        public static void DataTabletoExcel(System.Data.DataTable dt, string saveFile)
        {
            Microsoft.Office.Interop.Excel.Application rptExcel = new Microsoft.Office.Interop.Excel.Application();
            if (rptExcel == null)
            {
                throw new KDBusinessException("DataTabletoExcel", "无法打开EXcel，请检查Excel是否可用或者是否安装好Excel");
                return;
            }

            int rowCount = dt.Rows.Count;//行数
            int columnCount = dt.Columns.Count;//列数
            int rowIndex = 1;
            int colindex = 1;
            //保存文化环境
            System.Globalization.CultureInfo currentCI = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            Microsoft.Office.Interop.Excel.Workbook workbook = rptExcel.Workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
            Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets.get_Item(1);
            worksheet.Name = "最终配货计划报表";//一个sheet的名称
            rptExcel.Visible = true;//打开导出的Excel文件

            worksheet.Cells[1, 1] = "27705";//模版号
            rowIndex++;
            //第二行内容
            Microsoft.Office.Interop.Excel.Range rangeinfo1 = worksheet.get_Range(worksheet.Cells[rowIndex, colindex + 6], worksheet.Cells[rowIndex, colindex + 7]);
            rangeinfo1.NumberFormat = "@";
            worksheet.Cells[rowIndex, colindex] = "S#262229";
            worksheet.Cells[rowIndex, colindex + 6] = dt.Columns[13].ColumnName;
            worksheet.Cells[rowIndex, colindex + 7] = dt.Rows[0][13];
            worksheet.Cells[rowIndex, colindex + 12] = "EAN";
            //合并打印数量单元格
            worksheet.Cells[rowIndex, columnCount + 1] = "打印数量";
            worksheet.Cells[rowIndex, columnCount + 2] = "包装数量";
            worksheet.get_Range(worksheet.Cells[rowIndex, columnCount + 1], worksheet.Cells[rowIndex + 2, columnCount + 1]).MergeCells = true;
            worksheet.get_Range(worksheet.Cells[rowIndex, columnCount + 2], worksheet.Cells[rowIndex + 2, columnCount + 2]).MergeCells = true;
            rowIndex++;
            //第三行内容
            worksheet.Cells[rowIndex, 1] = "貨名";
            worksheet.Cells[rowIndex, 2] = "Line";
            //合并第三行第二列
            worksheet.get_Range(worksheet.Cells[rowIndex, 2], worksheet.Cells[rowIndex + 1, 2]).MergeCells = true;
            worksheet.Cells[rowIndex, 3] = "序號";
            worksheet.Cells[rowIndex, 4] = "數量";
            worksheet.Cells[rowIndex, 5] = "1 (EUR) size";
            worksheet.Cells[rowIndex, 6] = "1a (€)";
            worksheet.Cells[rowIndex, 7] = "2 (UK) size";
            worksheet.Cells[rowIndex, 8] = "2a (₤)";
            worksheet.get_Range(worksheet.Cells[rowIndex, 5], worksheet.Cells[rowIndex, 8]).Font.Bold = true;
            worksheet.Cells[rowIndex, 9] = "4";
            worksheet.Cells[rowIndex, 10] = "5";
            worksheet.Cells[rowIndex, 11] = "6";
            worksheet.Cells[rowIndex, 12] = "7";
            worksheet.Cells[rowIndex, 13] = "8(barcode)";
            worksheet.Cells[rowIndex, 14] = "9";
            worksheet.get_Range(worksheet.Cells[rowIndex, 5], worksheet.Cells[rowIndex, 8]).Font.ColorIndex = 5;
            rowIndex++;
            //填充列标题
            for (int i = 0; i < columnCount - 1; i++)
            {
                if (i > 0)
                {
                    worksheet.Cells[rowIndex, i + 2] = dt.Columns[i].ColumnName;


                }
                else
                {
                    worksheet.Cells[rowIndex, i + 1] = dt.Columns[i].ColumnName;

                }
            }
            rowIndex++;

            //创建对象数组存储DataTable的数据，这样的效率比直接将Datateble的数据填充worksheet.Cells[row,col]高
            object[,] objData = new object[rowCount, columnCount];

            //填充内容到对象数组
            for (int r = 0; r < rowCount; r++)
            {
                for (int col = 0; col < columnCount - 1; col++)
                {
                    objData[r, col] = dt.Rows[r][col].ToString();
                }

                //System.Windows.Forms.Application.DoEvents();
            }


            //将对象数组的值赋给Excel对象
            Microsoft.Office.Interop.Excel.Range range = worksheet.get_Range(worksheet.Cells[rowIndex, 1], worksheet.Cells[rowCount + rowIndex - 1, columnCount - 1]);
            range.NumberFormat = "@";//设置数字文本格式
            Microsoft.Office.Interop.Excel.Range rangeinfo = worksheet.get_Range(worksheet.Cells[rowIndex, 4], worksheet.Cells[rowCount + rowIndex - 1, 4]);
            rangeinfo.NumberFormat = "00";
            range.Value2 = objData;

            for (int i = 0; i < rowCount; i++)
            {
                if (i > 0)
                {

                    //计算打印数量
                    worksheet.Cells[rowIndex + i, columnCount + 1] = "=CEILING(D" + (rowIndex + i).ToString() + "*1.01+1,2)";

                }
                else
                {

                    worksheet.Cells[rowIndex + i, columnCount + 1] = "=CEILING(D" + (rowIndex + i).ToString() + "*1.01+1,2)";
                }
            }

            //设置格式
            rptExcel.StandardFont = "新細明體";
            rptExcel.StandardFontSize = 12;
            worksheet.get_Range(worksheet.Cells[1, 1], worksheet.Cells[rowCount + rowIndex, columnCount]).Columns.AutoFit();//设置单元格宽度为自适应
            worksheet.get_Range(worksheet.Cells[1, 1], worksheet.Cells[rowCount + rowIndex, columnCount]).HorizontalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;//居中对齐
            //worksheet.get_Range(worksheet.Cells[1, 1], worksheet.Cells[1, columnCount]).Font.Bold = true;
            //worksheet.get_Range(worksheet.Cells[1, 1], worksheet.Cells[1, columnCount]).Font.Color= ConsoleColor.Blue;
            // worksheet.get_Range(worksheet.Cells[2, 1], worksheet.Cells[rowCount + 2, columnCount]).Borders.LineStyle = 1;//设置边框
            //汇总
            rowIndex = rowCount + rowIndex;
            worksheet.Cells[rowIndex, 4] = "=SUM(D5:D10)";


            //恢复文化环境
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCI;
            try
            {
                //rptExcel.Save(saveFile); //自动创建一个新的Excel文档保存在“我的文档”里，如果不用SaveFileDialog就可用这种方法
                workbook.Saved = true;
                workbook.SaveCopyAs(saveFile);//以复制的形式保存在已有的文档里
               
                throw new KDBusinessException("DataTabletoExcel", "数据已经成功导出为Excel文件！");
            }
            catch (Exception ex)
            {
                throw new KDBusinessException("DataTabletoExcel", "导出文件出错，文件可能正被打开，具体原因：" + ex.Message);
            }
            finally
            {
                dt.Dispose();
                rptExcel.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rptExcel);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                GC.Collect();
                KillAllExcel();
            }
        }
        /// <summary>
        /// 获得所有的Excel进程
        /// </summary>
        /// <returns>所有的Excel进程</returns>
        private static List<Process> GetExcelProcesses()
        {
            Process[] processes = Process.GetProcesses();
            List<Process> excelProcesses = new List<Process>();

            for (int i = 0; i < processes.Length; i++)
            {
                if (processes[i].ProcessName.ToUpper() == "EXCEL")
                    excelProcesses.Add(processes[i]);
            }

            return excelProcesses;
        }
        private static void KillAllExcel()
        {
            List<Process> excelProcess = GetExcelProcesses();
            for (int i = 0; i < excelProcess.Count; i++)
            {
                excelProcess[i].Kill();
            }
        }
    }
}
