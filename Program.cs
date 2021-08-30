using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace CellSearchGenerator
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form());
        }
    }
}

namespace CellSearchGeneratorLogic
{
    public class Program
    {
        private static List<string> GenMaster(char wing)
        {
            //Returns a list of generated cells for the given wing
            List<string> cellsList = new();

            //We have to differentiate between pod sizes and dorm sizes
            if (wing.ToString() == "M" || wing.ToString() == "P")
            {
                for (int i = 1; i < 64; i++)
                {
                    string cell = String.Format("{0:00}", i);
                    cellsList.Add(cell);
                }
            }
            else if (wing.ToString() == "N" || wing.ToString() == "O")
            {
                for (int i = 1; i < 126; i++)
                {
                    string cell = String.Format("{0:00}", i);
                    cellsList.Add(cell);
                }
            }
            else
            {
                for (int i = 1; i < 22; i++)
                {
                    string cellOneRow = "1-" + String.Format("{0:00}", i);
                    string cellTwoRow = "2-" + String.Format("{0:00}", i);
                    cellsList.Add(cellOneRow);
                    cellsList.Add(cellTwoRow);
                }
            }

            return cellsList;
        }

        private static void GenPDF(Dictionary<string, List<string>> monthlyList, int month, int year, int daysTotal)
        {
            //Converting month int to longhand
            List<string> monthsLong = new()
            {
                "January",
                "February",
                "March",
                "April",
                "May",
                "June",
                "July",
                "August",
                "September",
                "October",
                "November",
                "December"
            };
            string monthLong = monthsLong[month - 1];

            //File destination
            string dest = String.Format(".\\Cell Searches\\{0:00}_{1}.pdf", month, year);
            string path = ".\\Cell Searches";
            if (!Directory.Exists(path))
            {
                _ = Directory.CreateDirectory(path);
            }

            //Initializing PdfWriter
            PdfWriter writer = new(dest);
            PdfDocument pdfDoc = new(writer);
            Document doc = new(pdfDoc);

            Table GenTable(bool J5)
            {
                //Creating master table to insert sub tables into
                Table masterTable = new Table(UnitValue.CreatePointArray(new float[] { 80, 140, 140, 140 })).UseAllAvailableWidth();
                Table masterTableJ5 = new Table(UnitValue.CreatePointArray(new float[] { 80, 100, 100, 100, 100 })).UseAllAvailableWidth();

                //Adding left hand title and month to the page headers
                Cell titleCell = new(1, 3);
                titleCell.Add(new Paragraph("T.L. Roach Unit Cell Search Schedule")).SetTextAlignment(TextAlignment.LEFT).SetBold().SetFontSize(16).SetBorder(Border.NO_BORDER);
                masterTable.AddHeaderCell(titleCell);
                Cell monthCell = new(1, 1);
                monthCell.Add(new Paragraph(String.Format("{0}", monthLong))).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER);
                masterTable.AddHeaderCell(monthCell);

                //Adding left hand title and month to the page headers for J5
                Cell titleCellJ5 = new(1, 4);
                titleCellJ5.Add(new Paragraph("T.L. Roach Unit Cell Search Schedule")).SetTextAlignment(TextAlignment.LEFT).SetBold().SetFontSize(16).SetBorder(Border.NO_BORDER);
                masterTableJ5.AddHeaderCell(titleCellJ5);
                Cell monthCellJ5 = new(1, 1);
                monthCellJ5.Add(new Paragraph(String.Format("{0}", monthLong))).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER);
                masterTableJ5.AddHeaderCell(monthCellJ5);

                if (J5)
                {
                    return masterTableJ5;
                }
                else
                {
                    return masterTable;
                }
            }

            //Creating dates sub table for the month for J1-J5
            Table dates = new Table(UnitValue.CreatePercentArray(1)).SetWidth(70).SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
            for (int i = 1; i < daysTotal + 1; i++)
            {
                string day = String.Format("{0:00}/{1:00}/{2}", month, i, year - 2000);
                dates.AddCell(day);
            }

            //Setting initial building number and initial tables
            int buildingNumber = 1;
            Table masterTable = GenTable(false);
            Table masterTableJ5 = GenTable(true);

            foreach (KeyValuePair<string, List<string>> kvp in monthlyList)
            {
                //Create datesCell to add later in loop
                Cell datesCell = new();
                datesCell.Add(new Paragraph("Date").SetBold().SetFontSize(10)).SetBorder(Border.NO_BORDER);
                datesCell.Add(dates);

                //Increment building number, add table for building, reset masterTable, and add a page break
                if (kvp.Key == "D" || kvp.Key == "G" || kvp.Key == "J" || kvp.Key == "M")
                {
                    buildingNumber += 1;
                    doc.Add(masterTable);
                    AreaBreak ab = new();
                    doc.Add(ab);
                    masterTable = GenTable(false);
                }

                //Create and add building headers
                if (kvp.Key == "A" || kvp.Key == "D" || kvp.Key == "G" || kvp.Key == "J")
                {
                    Cell buildingCell = new(1, 4);
                    buildingCell.Add(new Paragraph(String.Format("J{0} Building", buildingNumber))).SetFontSize(14).SetTextAlignment(TextAlignment.CENTER).SetBold().SetUnderline().SetBorder(Border.NO_BORDER);
                    masterTable.AddCell(buildingCell);
                    masterTable.AddCell(datesCell);
                }
                else if (kvp.Key == "M")
                {
                    Cell buildingCell = new(1, 5);
                    buildingCell.Add(new Paragraph(String.Format("J{0} Building", buildingNumber))).SetFontSize(14).SetTextAlignment(TextAlignment.CENTER).SetBold().SetUnderline().SetBorder(Border.NO_BORDER);
                    masterTableJ5.AddCell(buildingCell);
                    masterTableJ5.AddCell(datesCell);
                }

                //Create and add cell tables
                if (kvp.Key == "M" || kvp.Key == "N" || kvp.Key == "O" || kvp.Key == "P")
                {
                    Table cellsTable = new Table(UnitValue.CreatePercentArray(2)).SetWidth(100).SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                    foreach (string cell in kvp.Value)
                    {
                        cellsTable.AddCell(cell);
                    }
                    Cell cellsCell = new();
                    cellsCell.Add(new Paragraph(kvp.Key + "-WING").SetBold().SetFontSize(10)).SetBorder(Border.NO_BORDER);
                    cellsCell.Add(cellsTable);
                    masterTableJ5.AddCell(cellsCell).SetTextAlignment(TextAlignment.CENTER);
                }
                else
                {
                    Table cellsTable = new Table(UnitValue.CreatePercentArray(2)).SetWidth(140).SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                    foreach (string cell in kvp.Value)
                    {
                        cellsTable.AddCell(cell);
                    }
                    Cell cellsCell = new();
                    cellsCell.Add(new Paragraph(kvp.Key + "-WING").SetBold().SetFontSize(10)).SetBorder(Border.NO_BORDER);
                    cellsCell.Add(cellsTable);
                    masterTable.AddCell(cellsCell).SetTextAlignment(TextAlignment.CENTER);
                }
            }

            doc.Add(masterTableJ5);
            doc.Close();
        }

        public static void GenMain(string[] args)
        {
            //Initializing variables
            int month = Convert.ToInt32(args[0]);
            int year = Convert.ToInt32(args[1]);
            int daysTotal = DateTime.DaysInMonth(year, month);
            var random = new Random();
            const string wings = "ABCDEFGHIJKLMNOP";
            List<List<string>> masterList = new();
            Dictionary<string, List<string>> monthList = new();

            //Generating master list of cells seperated into sublists by wing
            foreach (char ch in wings)
            {
                masterList.Add(GenMaster(ch));
            }

            //Looping through every wing and generating a list of cells to search for each day
            for (int i = 0; i < masterList.Count; i++)
            {
                List<string> thisDay = new();

                for (int d = 0; d < daysTotal; d++)
                {
                    //If specific wing is empty, regenerate list
                    if (masterList[i].Count == 0)
                    {
                        masterList[i] = GenMaster(wings[i]);
                    }

                    //We need two cells per wing per day, so we choose/save two random cells and remove from the masterList
                    int index = random.Next(masterList[i].Count);
                    thisDay.Add(masterList[i][index]);
                    masterList[i].Remove(masterList[i][index]);

                    int index2 = random.Next(masterList[i].Count);
                    thisDay.Add(masterList[i][index2]);
                    masterList[i].Remove(masterList[i][index2]);
                }

                //Add current wing list to dictionary sorted by wing/cell list kvp
                monthList[wings[i].ToString()] = thisDay;
            }

            //Generate PDF
            GenPDF(monthList, month, year, daysTotal);
        }
    }
}