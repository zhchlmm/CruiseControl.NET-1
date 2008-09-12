using System;
using System.Collections;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;
using ThoughtWorks.CruiseControl.WebDashboard.Plugins.BuildReport;

namespace ThoughtWorks.CruiseControl.WebDashboard.Plugins.Statistics
{
	/// <summary>
	/// Provides functions for making a graph of the specified builds.
    /// These are HTML tables, so should not be browser specific.
	/// </summary>
    public class BuildGraph
    {
        private IBuildSpecifier[] mybuildSpecifiers;
        private ILinkFactory mylinkFactory;
        private Int32 myHighestAmountPerDay;
                
        public BuildGraph(IBuildSpecifier[] buildSpecifiers, ILinkFactory linkFactory)
        {
            mybuildSpecifiers = buildSpecifiers;
            mylinkFactory = linkFactory;
        }

        public Int32 HighestAmountPerDay
        {
            get
            {
                return myHighestAmountPerDay;
            }
        }
    

        public override bool Equals(object obj)
        {            
            if (obj.GetType() != this.GetType() )
                return false;

            BuildGraph Comparable = obj as BuildGraph;

            if (this.mybuildSpecifiers.Length != Comparable.mybuildSpecifiers.Length)
                {return false; }
        

            for (int i=0; i < this.mybuildSpecifiers.Length ; i++)
            {
                if (! this.mybuildSpecifiers[i].Equals(Comparable.mybuildSpecifiers[i]) )
                {return false; }
            }

            return true;
        }

        /// <summary>
        //Returns a sorted list containing build information per buildday
        /// </summary>
        public ArrayList GetBuildHistory(Int32 maxAmountOfDays)
        {
            ArrayList Result;
            ArrayList DateSorter;
            Hashtable FoundDates;
            GraphBuildInfo CurrentBuildInfo;
            GraphBuildDayInfo CurrentBuildDayInfo;


            // adding the builds to a list per day
            FoundDates = new Hashtable();
            DateSorter = new ArrayList();

            foreach (IBuildSpecifier buildSpecifier in mybuildSpecifiers)
            {           
                CurrentBuildInfo = new GraphBuildInfo(buildSpecifier, mylinkFactory);

                if (!FoundDates.Contains(CurrentBuildInfo.BuildDate()))
                {
                    FoundDates.Add(CurrentBuildInfo.BuildDate(), new GraphBuildDayInfo(CurrentBuildInfo) );
                    DateSorter.Add(CurrentBuildInfo.BuildDate());
                }
                else
                {
                    CurrentBuildDayInfo = FoundDates[CurrentBuildInfo.BuildDate()] as GraphBuildDayInfo;
                    CurrentBuildDayInfo.AddBuild(CurrentBuildInfo);

                    FoundDates[CurrentBuildInfo.BuildDate()] = CurrentBuildDayInfo;
                }                            
            }
 
            //making a sorted list of the dates where we have builds of
            //and limit to the amount specified in maxAmountOfDays
            DateSorter.Sort();
            while (DateSorter.Count > maxAmountOfDays)
            {
                DateSorter.RemoveAt(0);
            }

            //making final sorted arraylist
            Result = new ArrayList();
            myHighestAmountPerDay = 1;

            foreach (DateTime BuildDate in DateSorter)
            {
                CurrentBuildDayInfo = FoundDates[BuildDate] as GraphBuildDayInfo;
                Result.Add(CurrentBuildDayInfo);            

                if (CurrentBuildDayInfo.AmountOfBuilds > myHighestAmountPerDay) 
                {
                    myHighestAmountPerDay = CurrentBuildDayInfo.AmountOfBuilds; 
                }
            }

            return Result;
        }



        /// <summary>
        // Information about a certain build 
        // Wrapper around existing functions for ease of use in template
        /// </summary>
        public class GraphBuildInfo
        {
            private IBuildSpecifier mybuildSpecifier;
            private ILinkFactory mylinkFactory;

            public GraphBuildInfo(IBuildSpecifier buildSpecifier,  ILinkFactory linkFactory)
            {
                mybuildSpecifier = buildSpecifier;
                mylinkFactory = linkFactory;
            }
        
            //returns the day of the build (no time specification)            
            public DateTime BuildDate()
            {
                return new LogFile(mybuildSpecifier.BuildName).Date.Date;
            }

            public bool IsSuccesFull()
            {
                return new LogFile(mybuildSpecifier.BuildName).Succeeded;
            }

            public string LinkTobuild()
            {
                return mylinkFactory.CreateBuildLink( 
                    mybuildSpecifier,BuildReportBuildPlugin.ACTION_NAME).Url;                
            }

            public string Description()
            {               
                DefaultBuildNameFormatter BuildNameFormatter;
                BuildNameFormatter = new DefaultBuildNameFormatter();
                return BuildNameFormatter.GetPrettyBuildName(mybuildSpecifier);
            }

        }

        /// <summary>
        // structure containing all the builds on 1 day (YYYY-MM-DD)
        /// </summary>
        public class GraphBuildDayInfo
        {
            private DateTime myBuildDate; 
            private ArrayList myBuilds;

            public GraphBuildDayInfo(GraphBuildInfo buildInfo)
            {
                myBuildDate = buildInfo.BuildDate();
                myBuilds = new ArrayList();
                myBuilds.Add(buildInfo);
            }

            
            //returns the day of the builds contained
            public DateTime BuildDate
            {
                get 
                { 
                    return myBuildDate; 
                }
            }

            public string BuildDateFormatted
            {
                get 
                {
                    return myBuildDate.Date.ToString("ddd")
                           + "<BR>" 
                           + myBuildDate.Year.ToString("0000") 
                           + "<BR>" 
                           + myBuildDate.Month.ToString("00")
                           + "<BR>"
                           + myBuildDate.Day.ToString("00"); 
                }
            }


            // the amount of builds in this day
            public Int32 AmountOfBuilds
            {
                get
                {
                    return myBuilds.Count;
                }
            }

            //retrieves a specific build in this day
            public GraphBuildInfo Build(Int32 index)
            {
                return myBuilds[index] as GraphBuildInfo;
            }

            // adds a build to this day
            public void AddBuild(GraphBuildInfo buildInfo)
            {
                myBuilds.Insert(0, buildInfo);
            }
        }
	}
}