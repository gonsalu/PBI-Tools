// Author: Alexander Korn
// Created on: 5. December 2023
// Modified on 4. January 2024

// *********Description***********
// Script asks for and adds the following items:
// 1. Add a new Calculation Group for "Time Intelligence" Measures
// 2. Adding a Date Dimension Table, the table is automatically marked as date table
// 3. Adding an Empty Measure Table
// 4. Adding a Last Refresh Table
// 5. Formats the DAX of ALL calculation items in the model
// 6. All Key and ID columns: Set Summarize By to "None"
// 6. Adds Explicit Measures based on defined aggregation
// 7. Adding a Calc Group for "Units"
// 8. Checking for DiscourageImplicitMeasures and ask to set to true
// Variables to fill in:
//     - Calc Group Name,
//     - Date Table Name
//     - Date Column Name
//     - If YTD / FY measures shall be created and what the cutoff date for the FY is


#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
using System.Windows.Forms; 

// *********Variables to Modify from User if needed***********
// Here you can modify the names
string tableNameEmptyMeasure = "Measure";
string tableNameLastRefresh = "Last Refresh";
string columnNameLastRefresh = "Last Refreshes";
string LastRefreshMeasureName = "Last Refresh Measure";
var TimeIntelligenceCalculationGroupName = "Time Intelligence";
var CalcGroupUnitsName = "Units";

// Here you can modify if you need Fiscal Year Time Intelligence Calculation Items
bool GenerateYTD = true;     
bool GenerateFiscalYear = false; 
string fiscalYearEndDate = "07/31";

// do not modify below this line
// Script starts ***********************************************************

//Variables
bool addtables;
bool GenerateEmptyMeasureTable = false;
bool GenerateLastRefreshTable = false;
bool GenerateDateDimensionTable = false;


// Checking if adding tables is possible otherwise prompt user ****************
// normally this part is just skipped and not visible to the the user
try
{
    // Define the table name
    string tableName = "TestingTable";
    // Create a new table in the model
    Table table = Model.AddTable(tableName);
    // delete the table if adding was successful
    var tableToDelete = Model.Tables[tableName];
    tableToDelete.Delete();
    addtables = true;
}
catch (Exception ex)
{
    addtables = false;
    DialogResult dialogResult = MessageBox.Show(text:"Important Disclaimer: You are executing this script at your own risk!\n\nWarning: Adding tables will not be successful. \n\nInstead of directly opening your model from an open instance of Power BI Desktop please save the model.bim file locally and reopen it afterwards.\n\nYou can still continue to run the script to apply other fixes. \n\nError message: " + ex.Message,caption:"Adding Tables Unsuccessful", buttons:MessageBoxButtons.OKCancel);
        if (dialogResult == DialogResult.Cancel)
    {
        return; // Cancel the script
    }
}

// This is the first prompt to the user *****************************************************************************
if (addtables)
{
    DialogResult dialogResult10 = MessageBox.Show(text:"You are executing this script at your own risk!\n\nPlease make sure to save and reopen the model.bim file from the .pbix. The model.bim from the Power BI Project folder is currently not supported.\n\nThe following parameters can be defined directly within the script: \n-Names for Tables \n-Names for Calc Groups \n-Time Intelligence Details \n\nDo you would like to proceed?", caption:"Important Disclaimer", buttons:MessageBoxButtons.OKCancel);
    if (dialogResult10 == DialogResult.Cancel)
    {
        return; // Cancel the script
    }
    
    DialogResult dialogResult = MessageBox.Show(text:"Add Empty Measure table?\n\nThis will add just an empty table, used as container for all measures with subfolders.", caption:"Empty Measure Table", buttons:MessageBoxButtons.YesNo);
    GenerateEmptyMeasureTable = (dialogResult == DialogResult.Yes);
}

DialogResult dialogResult15 = MessageBox.Show(text:"Change the current aggregation to 'none' for all columns ending with 'Key' or 'ID'?\n\nThis is a best practice, but also helpful if you want to add explicit measures for all non key columns in a following step within this script.", caption:"Aggregation Key Columns", buttons:MessageBoxButtons.YesNo);
bool KeyColumnsAggregation = (dialogResult15 == DialogResult.Yes); 

DialogResult dialogResult13 = MessageBox.Show(text:"Add Explicit Measures based on current aggregation?\n\nMake sure to use the correct aggregation for all of your columns. \nProperty name to modify: Summarize By", caption:"Explicit Measure creation", buttons:MessageBoxButtons.YesNo);
bool ExplicitMeasure = (dialogResult13 == DialogResult.Yes); 

// This portion is only run if adding tables was possible **********************************
if (addtables)
{
    DialogResult dialogResult1 = MessageBox.Show(text:"Add Last Refresh table?\n\nThis also adds a measure which you can use in your report to display when the last refresh happend. \nThis simple approach does not work if you have incremental refresh / multiple partitions set up.", caption:"Last Refresh Table", buttons:MessageBoxButtons.YesNo);
    GenerateLastRefreshTable = (dialogResult1 == DialogResult.Yes); 

    DialogResult dialogResult2 = MessageBox.Show(text:"Add Date Dimension table? \n\nThe table is automatically marked as date table. \nBasis is a Power Query Script", caption:"Date Dimension Table", buttons:MessageBoxButtons.YesNo);
    GenerateDateDimensionTable = (dialogResult2 == DialogResult.Yes); 
}

// Getting the names of the calendar table and date column, this is also run if adding tables is not possible, because it is needed for the time intelligence calc group ************
var Table = Interaction.InputBox("Provide the name of the date dimension table","Table Name","Calendar");
var Column = Interaction.InputBox("Provide the name of the date column name","Column Name","Date");

// Asking the user if calc groups and formatting shall be applied ****************************************************************************************
DialogResult dialogResult0 = MessageBox.Show(text:"Add Time Intelligence Calc Group?\n\nThis calculation group will immediately provide the possibility for all measures to have various time intelligence calculation items for example measures like Delta to previous year (Y-1) as absolut, percent and of 100%", caption:"Time Intelligence Calculation Group", buttons:MessageBoxButtons.YesNo);
bool GenerateCalcGroupTimeInt = (dialogResult0 == DialogResult.Yes); 

DialogResult dialogResult12 = MessageBox.Show(text:"Add Units Calculation Group for thousands and millions?\n\nThis simply divides the 'Selectedmeasure()' by 1k and 1mio with 'DIVIDE'.", caption:"Units Calculation Group", buttons:MessageBoxButtons.YesNo);
bool GenerateCalcGroupUnits = (dialogResult12 == DialogResult.Yes); 

DialogResult dialogResult5 = MessageBox.Show(text:"Format ALL calculation items for the calculation groups? This is not formatting measures.", caption:"Format DAX calc items", buttons:MessageBoxButtons.YesNo);
bool FormatDAXCalcItems = (dialogResult5 == DialogResult.Yes); 

DialogResult dialogResult6 = MessageBox.Show(text:"Format ALL DAX measures? \n\nThis is formatting ALL measures, therefore if you apply this script to an existing model and you have a lot of measures this can be a rather big impact.", caption:"Format ALL measures", buttons:MessageBoxButtons.YesNo);
bool FormatDAX = (dialogResult6 == DialogResult.Yes);

DialogResult dialogResult16 = MessageBox.Show(text:"Load BPA into TE? \n\nThis loads initially or updates all rules into Tabular Editor for the Best Practice Analyzer (BPA). The BPA is superpowerful tool to optimize your data model even further.\n\nYou will need to REOPEN Tabular Editor for this part to be applied.", caption:"Load BPA", buttons:MessageBoxButtons.YesNo);
bool LoadBPA = (dialogResult16 == DialogResult.Yes);


// This portion is skipped if DiscourageImplicitMeasure is already set to true, otherwise the user is asked to change it*************************************************************
// This being set to true is needed for calculation groups to work.

if (!Model.DiscourageImplicitMeasures)
{
    // Show message box
    DialogResult dialogResult14 = MessageBox.Show(
        text: "Set DiscourageImplicitMeasures to true?\n\nThis is in general recommended and needed for calculation groups.", 
        caption: "Discourage Implicit Measures", 
        buttons: MessageBoxButtons.YesNo);

    // If user clicks Yes, set DiscourageImplicitMeasures to true
    if (dialogResult14 == DialogResult.Yes)
    {
        Model.DiscourageImplicitMeasures = true;
    }
}

// Show the result of all of the previous selection *************************************************************************************************************************
{
    DialogResult dialogResult11 = MessageBox.Show(text:
    "Tables:"+
    "\n1. Create Last Refresh Table: "+GenerateLastRefreshTable+
    "\n   with name: "+tableNameLastRefresh+
    "\n2. Create Empty Measure Table: "+GenerateEmptyMeasureTable+
    "\n   with name: "+tableNameEmptyMeasure+
    "\n3. Create Date Dimension Table: "+GenerateDateDimensionTable+
    "\n  3.1 Date Dimension Table Name: '"+Table+"'"+
    "\n  3.2 Date Dimension Column Name: '"+Column+"'"+
    
    "\n\nCalculation Groups:"+
    "\n4. Time Intelligence Calc Group: "+GenerateCalcGroupTimeInt+
    "\n   with name: "+TimeIntelligenceCalculationGroupName+
    "\n  4.1 YTD items: "+GenerateYTD+
    "\n  4.2 FiscalYear items: "+GenerateFiscalYear+
    "\n  4.3 FiscalYear cutoff date: "+fiscalYearEndDate+
    "\n5. Units Calc Group: "+GenerateCalcGroupUnits+
    "\n   with name: "+CalcGroupUnitsName+
    
    "\n\nMeasures:"+
    "\n6. Create all Explicit Measures: "+ExplicitMeasure+
    "\n7. Format all Measures: "+FormatDAX+
    "\n8. Format all Calculation Items: "+FormatDAXCalcItems+
    
    "\n\nOther:"+
    "\n9. Remove Aggregation for Key Columns: "+KeyColumnsAggregation+
    "\n10. Load BPA: "+LoadBPA
    
    
    ,caption:"Summary of Selected Parameters", buttons:MessageBoxButtons.OKCancel);
if (dialogResult11 == DialogResult.Cancel)
    {
        return; // Cancel the script
    }}

    
// Adding an empty measure table *********************************************************** 
if (GenerateEmptyMeasureTable)
        {
            try {

    // Create a new table in the model
    Table table = Model.AddTable(tableNameEmptyMeasure);
    // Add the "Name of Measure" column to the table
    DataColumn column1 = table.AddDataColumn();
    column1.Name = "Name of Measure";
    column1.DataType = DataType.String;
    column1.SourceColumn = "Name of Measure";
    column1.IsHidden = true; // Hide the column
    column1.SummarizeBy = AggregateFunction.None;
    // Add the "Description" column to the table
    DataColumn column2 = table.AddDataColumn();
    column2.Name = "Description";
    column2.DataType = DataType.String;
    column2.SourceColumn = "Description";
    column2.IsHidden = true; // Hide the column
    column2.SummarizeBy = AggregateFunction.None;

    if (!Model.Tables.Any(t => t.Name == tableNameEmptyMeasure))
    {
        throw new InvalidOperationException("Empty measure table does not exist in the model.");
    }

                string mExpression = @"
    let
        Source = Table.FromRows(Json.Document(Binary.Decompress(Binary.FromText(""i44FAA=="", BinaryEncoding.Base64), Compression.Deflate)), let _t = ((type nullable text) meta [Serialized.Text = true]) in type table [#""Name of Measure"" = _t, Description = _t])
    in
        Source";


        // Update existing partition
    var partition = table.Partitions.First();
    partition.Expression = mExpression;
            partition.Mode = ModeType.Import; // Set the refresh policy to Import
    }
    catch (Exception ex)
    {MessageBox.Show("Adding Empty Measure table was not successful but the rest of the script was completed\n\nReason: "+ex.Message);
        }
    }


// Change SummarizeBy to None for All ID and Key columns ***********************************************************
if (KeyColumnsAggregation)
    {
    foreach (var table in Model.Tables)
    {
        foreach (var column in table.Columns)
        {
            if (column.Name.EndsWith("Key") || column.Name.EndsWith("ID"))
            {
                column.SummarizeBy = AggregateFunction.None;
            }
        }
    }
    }

// Create Explicit Measures for all tables for all columns with summarize by in the empty measure folder. ******************
if (ExplicitMeasure)
   {
// Title: Auto-create explicit measures from all columns in all tables that have qualifying aggregation functions assigned 
// Author of this part: Tom Martens, twitter.com/tommartens68
// Edited on 24/01/04 by: Alexander Korn (e.g. moving all measures to the empty measure table, creating one if has not been previously created, hiding all columns) 
//  
// This script, when executed, will loop through all the tables and creates explicit measure for all the columns with qualifying
// aggregation functions.
// The qualifying aggregation functions are SUM, COUNT, MIN, MAX, AVERAGE.
// This script can create a lot of measures, as by default the aggregation function for columns with a numeric data type is SUM.
// So, it is a good idea to check all columns for the proper aggregation type, e.g. the aggregation type of id columns 
// should be set to None, as it does not make any sense to aggregate id columns.
// An annotation:CreatedThrough is created with a value:CreateExplicitMeasures this will help to identify the measures created
// using this script.
// What is missing, the list below shows what might be coming in subsequent iterations of the script:
// - the base column property hidden is not set to true
// - no black list is used to prevent the creation of unwanted measures

// ***************************************************************************************************************
//the following variables are allowing controling the script
var overwriteExistingMeasures = 0; // 1 overwrites existing measures, 0 preserves existing measures

var measureNameTemplate = "{0} ({1}) ";
//"{0} ({1}) - {2}"; // String.Format is used to create the measure name. 
//{0} will be replaced with the columnname (c.Name), {1} will be replaced with the aggregation function, and last but not least
//{2} will be replaced with the tablename (t.Name). Using t.Name is necessary to create a distinction between measure names if
//columns with the same name exist in different tables.
//Assuming the column name inside the table "Fact Sale" is "Sales revenue" and the aggregation function is SUM 
//the measure name will be: "Sales revenue (Sum) - Fact Sale"

//store aggregation function that qualify for measure creation to the hashset aggFunctions
var aggFunctions = new HashSet<AggregateFunction>{
    AggregateFunction.Default, //remove this line, if you do not want to mess up your measures list by automatically created measures for all the columns that have the Default AggregateFunction assigned
    AggregateFunction.Sum,
    AggregateFunction.Count,
    AggregateFunction.Min,
    AggregateFunction.Max,
    AggregateFunction.Average
};

//You have to be aware that by default this script will just create measures using the aggregate functions "Sum" or "Count" if
//the column has the aggregate function AggregateFunction.Default assigned, this is checked further down below.
//Also, if a column has the Default AggregateFunction assigned and is of the DataType
//DataType.Automatic, DataType.Unknown, or DataType.Variant, no measure is created automatically, this is checked further down below.
//dictDataTypeAggregateFunction = new Dictionary<DataType, string>();
//see this article for all the available data types: https://docs.microsoft.com/en-us/dotnet/api/microsoft.analysisservices.tabular.datatype?view=analysisservices-dotnet
//Of course you can change the aggregation function that will be used for different data types,
//as long as you are using "Sum" and "Count"
//Please be careful, if you change the aggregation function you might end up with multiplemeasures
var dictDataTypeAggregateFunction = new Dictionary<DataType, AggregateFunction>();
dictDataTypeAggregateFunction.Add( DataType.Binary , AggregateFunction.Count ); //adding a key/value pair(s) to the dictionary using the Add() method
dictDataTypeAggregateFunction.Add( DataType.Boolean , AggregateFunction.Count );
dictDataTypeAggregateFunction.Add( DataType.DateTime , AggregateFunction.Count );
dictDataTypeAggregateFunction.Add( DataType.Decimal , AggregateFunction.Sum );
dictDataTypeAggregateFunction.Add( DataType.Double , AggregateFunction.Sum );
dictDataTypeAggregateFunction.Add( DataType.Int64 , AggregateFunction.Sum );
dictDataTypeAggregateFunction.Add( DataType.String , AggregateFunction.Count );

// ***************************************************************************************************************
//all the stuff below this line should not be altered 
//of course this is not valid if you have to fix my errors, make the code more efficient, 
//or you have a thorough understanding of what you are doing

//store all the existing measures to the list listOfMeasures
var listOfMeasures = new List<string>();
foreach( var m in Model.AllMeasures ) {
    listOfMeasures.Add( m.Name );
}

// Check if the "Measure" table exists, if not, create it
Table measureTable;
if (!Model.Tables.Any(t => t.Name == tableNameEmptyMeasure)) {
    measureTable = Model.AddTable(tableNameEmptyMeasure);
} else {
    measureTable = Model.Tables.First(t => t.Name == tableNameEmptyMeasure);
}

//loop across all tables
foreach( var t in Model.Tables ) {
    
    //loop across all columns of the current table t
    foreach( var c in t.Columns ) {
        
        var currAggFunction = c.SummarizeBy; //cache the aggregation function of the current column c
        var useAggFunction = AggregateFunction.Sum;
        var theMeasureName = ""; // Name of the new Measure
        var posInListOfMeasures = 0; //check if the new measure already exists <> -1
        
        if( aggFunctions.Contains(currAggFunction) ) //check if the current aggregation function qualifies for measure aggregation
        {
            //check if the current aggregation function is Default
            if( currAggFunction == AggregateFunction.Default )
            {
                // check if the datatype of the column is considered for measure creation
                if( dictDataTypeAggregateFunction.ContainsKey( c.DataType ) )
                {
                    
                    //some kind of sanity check
                    if( c.DataType == DataType.Automatic || c.DataType == DataType.Unknown || c.DataType == DataType.Variant )
                    {
                        Output("No measure will be created for columns with the data type: " + c.DataType.ToString() + " (" + c.DaxObjectFullName + ")");
                        continue; //moves to the next item in the foreach loop, the next colum in the current table
                    }
                  
                    //cache the aggregation function from the dictDataTypeAggregateFunction
                    useAggFunction = dictDataTypeAggregateFunction[ c.DataType ];
                    
                    //some kind of sanity check
                    if( useAggFunction != AggregateFunction.Count && useAggFunction != AggregateFunction.Sum ) 
                    {    
                        Output("No measure will be created for the column: " + c.DaxObjectFullName);
                        continue; //moves to the next item in the foreach loop, the next colum in the current table
                    }
                    theMeasureName = String.Format( measureNameTemplate , c.Name , useAggFunction.ToString() , t.Name ); // Name of the new Measure
                    posInListOfMeasures = listOfMeasures.IndexOf( theMeasureName ); //check if the new measure already exists <> -1
                    
                } else {
                   
                    continue; //moves to the next item in the foreach loop, the next colum in the current table
                }
                        
            } else {
                
                useAggFunction = currAggFunction;    
                theMeasureName = String.Format( measureNameTemplate , c.Name , useAggFunction.ToString() , t.Name ); // Name of the new Measure
                posInListOfMeasures = listOfMeasures.IndexOf( theMeasureName ); //check if the new measure already exists <> -1
            }
            
            //sanity check
            if(theMeasureName == "")
            {
                continue; //moves to the next item in the foreach loop, the next colum in the current table
            }
            
            // create the measure
            if( ( posInListOfMeasures == -1 || overwriteExistingMeasures == 1 )) 
            {    
                if( overwriteExistingMeasures == 1 ) 
                {
                    foreach( var m in Model.AllMeasures.Where( m => m.Name == theMeasureName ).ToList() ) 
                    {
                        m.Delete();
                    }
                }
                
                var newMeasure = measureTable.AddMeasure
                (
                    theMeasureName                                                                      // Name of the new Measure
                    , "" + useAggFunction.ToString().ToUpper() + "(" + c.DaxObjectFullName + ")"        // DAX expression
                    , t.DaxObjectFullName.Replace("'", "")+c.DisplayFolder
                );
                
                c.IsHidden = true;
                newMeasure.SetAnnotation( "CreatedThrough" , "CreateExplicitMeasures" ); // flag the measures created through this script
                
            }
        }    
    } }       
}

// Creates Calculation Group for Units *******************************************************************************
// only sticked with k and mio because billion in English is trillion in other languages such as German
    
if (GenerateCalcGroupUnits)
    {
// Add a new Units Calculation Group 
try{
var calcGroup = Model.AddCalculationGroup();
calcGroup.Name = CalcGroupUnitsName;
calcGroup.Columns["Name"].Name = CalcGroupUnitsName;
// Define calculation item data
var calculationItemData = new[]
{
    new { Name = "number", Expression = "SELECTEDMEASURE()" },
    new { Name = "k", Expression = "DIVIDE(SELECTEDMEASURE(), 1000)" },
    new { Name = "mio", Expression = "DIVIDE(SELECTEDMEASURE(), 1000000)" }
}.Where(item => item != null).ToArray();

// Add calculation items to the Calculation Group
foreach (var itemData in calculationItemData)
{
    var item = calcGroup.AddCalculationItem();
    item.Name = itemData.Name;
    item.Expression = itemData.Expression;
}
}
catch (Exception ex)
{MessageBox.Show("Adding the calc group units was not fully successful, but the rest of the script was completed\n\nReason: "+ex.Message);
    }

    }
    

// Creates Calculation Group for Time Intelligence *******************************************************************************
if (GenerateCalcGroupTimeInt)
        {
    /* Uncomment here if you want input boxes for the following four variables, already defined all the way at the beginning as text within this script
    var TimeIntelligenceCalculationGroupName = Interaction.InputBox("Provide the name of the calculation group name","Calc group","Time Intelligence");

    DialogResult dialogResult3 = MessageBox.Show(text:"Generate Fiscal Year Calc Items?", caption:"Calc Group: Fiscal Year", buttons:MessageBoxButtons.YesNo);
    bool GenerateFiscalYear = (dialogResult3 == DialogResult.Yes);

    if (GenerateFiscalYear)
    {fiscalYearEndDate = Interaction.InputBox("Enter the fiscal year end date (MM/DD):", "Fiscal Year End Date", fiscalYearEndDate);}

    DialogResult dialogResult4 = MessageBox.Show(text:"Generate YTD Calc Items?", caption:"Calc Group: YTD", buttons:MessageBoxButtons.YesNo);
    bool GenerateYTD = (dialogResult4 == DialogResult.Yes);            
    */

    // Add a new Time Intellignce Calculation Group **************************************************
    try{
    var calcGroup = Model.AddCalculationGroup();
    calcGroup.Name = TimeIntelligenceCalculationGroupName;
    calcGroup.Columns["Name"].Name = TimeIntelligenceCalculationGroupName; 
    // Define calculation item data
    var calculationItemData = new[]
    {
        new { Name = "AC", Expression = "SELECTEDMEASURE()" },
        new { Name = "Y-1", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0}))", Table, Column) },
        new { Name = "Y-2", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0}))", Table, Column) },
        new { Name = "Y-3", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -3, YEAR), ALL({0}))", Table, Column) },
        GenerateYTD ? new { Name = "YTD", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0}))", Table, Column) }: null,
        GenerateYTD ? new { Name = "YTD-1", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0}))", Table, Column) }: null,
        GenerateYTD ? new { Name = "YTD-2", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0}))", Table, Column) }: null,
        new { Name = "abs. AC vs Y-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0})) RETURN AC - Y1", Table, Column) },
        new { Name = "abs. AC vs Y-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0})) RETURN AC - Y2", Table, Column) },
        GenerateYTD ? new { Name = "abs. AC vs YTD-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0})) RETURN AC - Y1", Table, Column) }: null,
        GenerateYTD ? new { Name = "abs. AC vs YTD-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0})) RETURN AC - Y2", Table, Column) }: null,
        new { Name = "AC vs Y-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), SAMEPERIODLASTYEAR({0}[{1}]), ALL({0})) RETURN DIVIDE(AC - Y1, Y1)", Table, Column) },
        new { Name = "AC vs Y-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD({0}[{1}], -2, YEAR), ALL({0})) RETURN DIVIDE(AC - Y2, Y2)", Table, Column) },
        GenerateYTD ? new { Name = "AC vs YTD-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0})) RETURN DIVIDE(AC - Y1, Y1)", Table, Column) }: null,
        GenerateYTD ? new { Name = "AC vs YTD-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0})) RETURN DIVIDE(AC - Y2, Y2)", Table, Column) }: null,
        new { Name = "achiev. AC vs Y-1", Expression = string.Format("VAR AC = SELECTEDMEASURE() VAR Y1 = CALCULATE(SELECTEDMEASURE(), SAMEPERIODLASTYEAR({0}[{1}]), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y1 - AC ), 0 ) / Y1 ) )", Table, Column) },
        new { Name = "achiev. AC vs Y-2", Expression = string.Format("VAR AC = SELECTEDMEASURE() VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD({0}[{1}], -2, YEAR), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y2 - AC ), 0 ) / Y2 ) )", Table, Column) },
        GenerateYTD ? new { Name = "achiev. AC vs YTD-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y1 - AC ), 0 ) / Y1 ) )", Table, Column) }: null,
        GenerateYTD ? new { Name = "achiev. AC vs YTD-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y2 - AC ), 0 ) / Y2 ) )", Table, Column) }: null,
        GenerateFiscalYear ? new { Name = "FYTD", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0}))", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "FYTD-1", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -1, YEAR), ALL({0}))", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "FYTD-2", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -2, YEAR), ALL({0}))", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "abs. AC vs FYTD-1", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -1, YEAR), ALL({0})) RETURN AC - Y1", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "abs. AC vs FYTD-2", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -2, YEAR), ALL({0})) RETURN AC - Y2", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "AC vs FYTD-1", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -1, YEAR), ALL({0})) RETURN DIVIDE(AC - Y1, Y1)", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "AC vs FYTD-2", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -2, YEAR), ALL({0})) RETURN DIVIDE(AC - Y2, Y2)", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "achiev. AC vs FYTD-1", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -1, YEAR), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y1 - AC ), 0 ) / Y1 ) )", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "achiev. AC vs FYTD-2", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -2, YEAR), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y2 - AC ), 0 ) / Y2 ) )", Table, Column, fiscalYearEndDate) } : null
    }.Where(item => item != null).ToArray();
    // Add calculation items to the Calculation Group
    foreach (var itemData in calculationItemData)
    {
        var item = calcGroup.AddCalculationItem();
        item.Name = itemData.Name;
        item.Expression = itemData.Expression;
    }
    }
    catch (Exception ex)
    {MessageBox.Show("Adding the calc group time intelligence was not fully successful, but the rest of the script was completed\n\nReason: "+ex.Message);
        }  
    }

// Formats the DAX of all calculation items in calc groups. Those are not measures ************************************************************
if (FormatDAXCalcItems)
    {
    // DAX Formatting all Measures
    FormatDax(Model.AllCalculationItems);
    }

// Formats the DAX of all measures.  *************************************************************************************************************************
if (FormatDAX)
    {
    // DAX Formatting all Measures
    FormatDax(Model.AllMeasures);
    }


// Creates a last Refresh Table *******************************************************************************************************
    if (GenerateLastRefreshTable)
        {
            try {
    // Script adds a Last Refresh Table:
    // Create a new table in the model
    Table table = Model.AddTable(tableNameLastRefresh);

    string measureDax = "\"Last Refresh: \" & MAX('" + tableNameLastRefresh + "'[" + columnNameLastRefresh + "])";


    // Add the "Column1" column to the table
    DataColumn column1 = table.AddDataColumn();
    column1.Name = "Column1";
    column1.DataType = DataType.String;
    column1.SourceColumn = "Column1";
    column1.IsHidden = true; // Hide the column
    // Check if the table exists in the model
    if (!Model.Tables.Any(t => t.Name == tableNameLastRefresh))
    {
        throw new InvalidOperationException("Table Last Refresh does not exist in the model.");
    }
    // Define the M expression
    string mExpression = @"
    let
    #""Today"" = #table({""" + columnNameLastRefresh + @"""}, {{DateTime.From(DateTime.LocalNow())}})
    in
        #""Today"" ";
            // Update existing partition
            var partition = table.Partitions.First();
            partition.Expression = mExpression;
            partition.Mode = ModeType.Import; // Set the refresh policy to Import
            
      // Creates a last Refresh Measure ****************************************************************************
      var table2 = Model.Tables[tableNameEmptyMeasure];
      var measurelastrefresh = table2.AddMeasure(LastRefreshMeasureName, measureDax, "Meta");
    }
    catch (Exception ex)
    {MessageBox.Show("Adding the Last Refresh table was not successful but the rest of the script was completed\n\nReason: "+ex.Message);
        }
    }


// Creates a Date Dimension table *******************************************************************************
if (GenerateDateDimensionTable)
        {
            try {
    // Script adds a Date Dimension Table:
    // Create a new table in the model
    Table table = Model.AddTable(Table);
    table.DataCategory = "Time";
    // Add columns with specified names and data types, including SourceColumn
    DataColumn dateColumn = table.AddDataColumn();
    dateColumn.Name = Column;
    dateColumn.DataType = DataType.DateTime;
    dateColumn.IsKey = true;
    dateColumn.SourceColumn = "Date";
    DataColumn yearColumn = table.AddDataColumn();
    yearColumn.Name = "Year";
    yearColumn.DataType = DataType.Int64;
    yearColumn.SourceColumn = "Year";
    DataColumn monthColumn = table.AddDataColumn();
    monthColumn.Name = "Month";
    monthColumn.DataType = DataType.Int64;
    monthColumn.SourceColumn = "Month";
    DataColumn dayColumn = table.AddDataColumn();
    dayColumn.Name = "Day";
    dayColumn.DataType = DataType.Int64;
    dayColumn.SourceColumn = "Day";
    DataColumn dayNameColumn = table.AddDataColumn();
    dayNameColumn.Name = "DayName";
    dayNameColumn.DataType = DataType.String;
    dayNameColumn.SourceColumn = "DayName";
    DataColumn monthNameColumn = table.AddDataColumn();
    monthNameColumn.Name = "MonthName";
    monthNameColumn.DataType = DataType.String;
    monthNameColumn.SourceColumn = "MonthName";
    DataColumn quarterColumn = table.AddDataColumn();
    quarterColumn.Name = "Quarter";
    quarterColumn.DataType = DataType.Int64;
    quarterColumn.SourceColumn = "Quarter";
    DataColumn weekOfYearColumn = table.AddDataColumn();
    weekOfYearColumn.Name = "WeekOfYear";
    weekOfYearColumn.DataType = DataType.Int64;
    weekOfYearColumn.SourceColumn = "WeekOfYear";
    DataColumn yearMonthColumn = table.AddDataColumn();
    yearMonthColumn.Name = "YearMonth";
    yearMonthColumn.DataType = DataType.String;
    yearMonthColumn.SourceColumn = "YearMonth";
    DataColumn yearMonthCodeColumn = table.AddDataColumn();
    yearMonthCodeColumn.Name = "YearMonth Code";
    yearMonthCodeColumn.DataType = DataType.String;
    yearMonthCodeColumn.SourceColumn = "YearMonth Code";
    
    // Check if the table exists in the model
    if (!Model.Tables.Any(t => t.Name == Table))
    {
        throw new InvalidOperationException("Table Date Dimension does not exist in the model.");
    }
    // Define the M expression
    string mExpression = @"
    let
        // configurations start
        Today=Date.From(DateTime.LocalNow()), // today's date
        FromYear = 2018, // set the start year of the date dimension. dates start from 1st of January of this year
        ToYear=2025, // set the end year of the date dimension. dates end at 31st of December of this year
        StartofFiscalYear=7, // set the month number that is start of the financial year. example; if fiscal year start is July, value is 7
        firstDayofWeek=Day.Monday, // set the week's start day, values: Day.Monday, Day.Sunday....
        // configuration end
        FromDate=#date(FromYear,1,1),
        ToDate=#date(ToYear,12,31),
        Source=List.Dates(
            FromDate,
            Duration.Days(ToDate-FromDate)+1,
            #duration(1,0,0,0)
        ),
        #""Converted to Table"" = Table.FromList(Source, Splitter.SplitByNothing(), null, null, ExtraValues.Error),
        #""Renamed Columns"" = Table.RenameColumns(#""Converted to Table"",{{""Column1"", ""Date""}}),
        #""Changed Type"" = Table.TransformColumnTypes(#""Renamed Columns"",{{""Date"", type date}}),
        #""Added Custom"" = Table.AddColumn(#""Changed Type"", ""Custom"", each [
            Year = Date.Year([Date]),
            StartOfYear = Date.StartOfYear([Date]),
            EndOfYear = Date.EndOfYear([Date]),
            Month = Date.Month([Date]),
            StartOfMonth = Date.StartOfMonth([Date]),
            EndOfMonth = Date.EndOfMonth([Date]),
            DaysInMonth = Date.DaysInMonth([Date]),
            Day = Date.Day([Date]),
            DayName = Date.DayOfWeekName([Date]),
            DayOfWeek = Date.DayOfWeek([Date], firstDayofWeek),
            DayOfYear = Date.DayOfYear([Date]),
            MonthName = Date.MonthName([Date]),
            Quarter = Date.QuarterOfYear([Date]),
            StartOfQuarter = Date.StartOfQuarter([Date]),
            EndOfQuarter = Date.EndOfQuarter([Date]),
            WeekOfYear = Date.WeekOfYear([Date], firstDayofWeek),
            WeekOfMonth = Date.WeekOfMonth([Date], firstDayofWeek),
            StartOfWeek = Date.StartOfWeek([Date], firstDayofWeek),
            EndOfWeek = Date.EndOfWeek([Date], firstDayofWeek)
        ]),
        #""Expanded Custom"" = Table.ExpandRecordColumn(#""Added Custom"", ""Custom"", {""Year"", ""StartOfYear"", ""EndOfYear"", ""Month"", ""StartOfMonth"", ""EndOfMonth"", ""DaysInMonth"", ""Day"", ""DayName"", ""DayOfWeek"", ""DayOfYear"", ""MonthName"", ""Quarter"", ""StartOfQuarter"", ""EndOfQuarter"", ""WeekOfYear"", ""WeekOfMonth"", ""StartOfWeek"", ""EndOfWeek""}, {""Year"", ""StartOfYear"", ""EndOfYear"", ""Month"", ""StartOfMonth"", ""EndOfMonth"", ""DaysInMonth"", ""Day"", ""DayName"", ""DayOfWeek"", ""DayOfYear"", ""MonthName"", ""Quarter"", ""StartOfQuarter"", ""EndOfQuarter"", ""WeekOfYear"", ""WeekOfMonth"", ""StartOfWeek"", ""EndOfWeek""}),
        FiscalMonthBaseIndex=13-StartofFiscalYear,
        adjustedFiscalMonthBaseIndex=if(FiscalMonthBaseIndex>=12 or FiscalMonthBaseIndex<0) then 0 else FiscalMonthBaseIndex,
        #""Added CustomA"" = Table.AddColumn(#""Expanded Custom"", ""FiscalBaseDate"", each Date.AddMonths([Date],adjustedFiscalMonthBaseIndex)),
        #""Changed Type2"" = Table.TransformColumnTypes(#""Added CustomA"",{{""FiscalBaseDate"", type date}}),
        #""Added CustomB"" = Table.AddColumn(#""Changed Type2"", ""Custom2"", each [
            Fiscal Year = Date.Year([FiscalBaseDate]),
            Fiscal Quarter = Date.QuarterOfYear([FiscalBaseDate]),
            Fiscal Month = Date.Month([FiscalBaseDate]),
            YearMonth = Date.ToText([Date], ""yyyy MMM""),
            YearMonth Code = Date.ToText([Date], ""yyyyMM"")
        ]),
        #""Expanded Custom2"" = Table.ExpandRecordColumn(#""Added CustomB"", ""Custom2"", {""Fiscal Year"", ""Fiscal Quarter"", ""Fiscal Month"", ""Age"", ""Month Offset"", ""Year Offset"", ""Quarter Offset"", ""YearMonth"", ""YearMonth Code""}, {""Fiscal Year"", ""Fiscal Quarter"", ""Fiscal Month"", ""Age"", ""Month Offset"", ""Year Offset"", ""Quarter Offset"", ""YearMonth"", ""YearMonth Code""}),
        #""Extracted Days"" = Table.TransformColumns(#""Expanded Custom2"",{{""Age"", Duration.Days, Int64.Type}}),
        #""Renamed Columns1"" = Table.RenameColumns(#""Extracted Days"",{{""Age"", ""Day Offset""}}),
        #""Changed Type1"" = Table.TransformColumnTypes(#""Renamed Columns1"",{{""StartOfYear"", type date}, {""EndOfYear"", type date}, {""StartOfMonth"", type date}, {""EndOfMonth"", type date}, {""StartOfQuarter"", type date}, {""EndOfQuarter"", type date}, {""StartOfWeek"", type date}, {""EndOfWeek"", type date}}),
        #""Removed Other Columns"" = Table.SelectColumns(#""Changed Type1"",{""Date"", ""Year"", ""Month"", ""Day"", ""DayName"", ""MonthName"", ""Quarter"", ""WeekOfYear"", ""YearMonth"", ""YearMonth Code""}),
    #""Renamed Columns2"" = Table.RenameColumns(#""Extracted Days"",{{""Date"", """ + Column + @"""}})
    in
        #""Renamed Columns2"" ";

    // Update existing partition
    var partition = table.Partitions.First();
    partition.Expression = mExpression;
    partition.Mode = ModeType.Import; // Set the refresh policy to Import
    }
    catch (Exception ex)
    {MessageBox.Show("Adding the Date Dimension Table was not successful but the rest of the script was completed\n\nReason: "+ex.Message);
        }
    }

//Load or Update BPA Rules into Tabular Editor
if (LoadBPA)
        {
System.Net.WebClient w = new System.Net.WebClient(); 

string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
string url = "https://raw.githubusercontent.com/microsoft/Analysis-Services/master/BestPracticeRules/BPARules.json";
string downloadLoc = path+@"\TabularEditor\BPARules.json";
w.DownloadFile(url, downloadLoc);
}