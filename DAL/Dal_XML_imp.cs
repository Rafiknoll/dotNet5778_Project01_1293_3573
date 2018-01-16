﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;
using System.IO;
using System.Xml.Linq;

namespace DAL
{
    public class Dal_XML_imp : Idal
    {
        static int SerialNum = 10000000; //needs to be removed
        static string Address = Directory.GetParent((Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName)).FullName).FullName + "\\DAL\\Resources";
        static XElement ChildrenRoot;
        static XElement MothersRoot;
        static XElement NanniesRoot;
        static XElement ContractsRoot;
        protected enum TypeToLoad { Nanny, Mother, Child, Contract}

        static Dal_XML_imp instance = null;
        public static Dal_XML_imp Instance
        {
            get
            {
                if (instance == null)
                    instance = new Dal_XML_imp();
                return instance;
            }
        }

        protected void LoadData(TypeToLoad t)
        {
            try
            {
                switch(t)
                {
                    case TypeToLoad.Child:
                        ChildrenRoot = XElement.Load(Address + "\\Children.xml");
                        break;
                    case TypeToLoad.Contract:
                        ContractsRoot = XElement.Load(Address + "\\Contracts.xml");
                        break;
                    case TypeToLoad.Mother:
                        MothersRoot = XElement.Load(Address + "\\Mothers.xml");
                        break;
                    case TypeToLoad.Nanny:
                        NanniesRoot = XElement.Load(Address + "\\Nannies.xml");
                        break;
                }    
            }
            catch (Exception)
            {
                //throw new Exception("File upload failed");
                ChildrenRoot = new XElement("Children");
                ContractsRoot = new XElement("Contracts");
                MothersRoot = new XElement("Mothers");
                NanniesRoot = new XElement("Nannies");
            }
        }
        public void AddChild(Child c)
        {
            LoadData(TypeToLoad.Child);
            foreach (Nanny n in GetNannies())
                if (n.ID == c.ID)
                    throw new Exception("ID already exists!");
            foreach (Child ch in GetChildren())
                if (ch.ID == c.ID)
                    throw new Exception("ID already exists!");
            foreach (Mother m in GetMothers())
                if (m.ID == c.ID)
                    throw new Exception("ID already exists!");
            if (FindMotherByID(c.MotherID) == null)
                throw new Exception("mother doesn't exist!");
            XElement newChild = new XElement("Child",
                new XElement("ID", c.ID),
                new XElement("MotherID", c.MotherID),
                new XElement("FirstName", c.FirstName),
                new XElement("LastName", c.LastName),
                new XElement("HaveSpecialNeeds", c.HaveSpecialNeeds),
                new XElement("BirthDate", new XElement("Year", c.BirthDate.Year), new XElement("Month", c.BirthDate.Month), new XElement("Day", c.BirthDate.Day)),
                new XElement("SpecialNeeds"));
            foreach (string need in c.SpecialNeeds)
                newChild.Element("SpecialNeeds").Add(new XElement("need", need));
            ChildrenRoot.Add(newChild);
            ChildrenRoot.Save(Address + "\\Children.xml");
        }

        public void AddContract(Contract c)
        {
            LoadData(TypeToLoad.Contract);
            Nanny NannyInContract = FindNannyByID(c.NannyID);
            Child ChildInContract = FindChildByID(c.ChildID);
            if (NannyInContract == null)
                throw new Exception("Nanny doesn't exist!");
            if (ChildInContract == null)
                throw new Exception("Mother doesn't exist!"); // because a child always has a mother, lack of child is the only way to miss a mother
            foreach (Contract con in GetContracts())
                if (con.ChildID == c.ChildID && con.NannyID == c.NannyID)
                    throw new Exception("Contarct between this nanny and child already exists");
            c.SerialNumber = (SerialNum++).ToString();            
            XElement newContract = new XElement("Contract", 
                new XElement("SerialNumber", c.SerialNumber),
                new XElement("ChildID", c.ChildID),
                new XElement("NannyID", c.NannyID),
                new XElement("PerHour", c.PerHour),
                new XElement("PerMonth", c.PerMonth),
                new XElement("IsByHour", c.IsByHour),
                new XElement("IsMet", c.IsMet),
                new XElement("IsSigned", c.IsSigned),
                new XElement("Beginning", new XElement("Year", c.Beginning.Year), new XElement("Month", c.Beginning.Month), new XElement("Day", c.Beginning.Day)),
                new XElement("End", new XElement("Year", c.End.Year), new XElement("Month", c.End.Month), new XElement("Day", c.End.Day)));
            ContractsRoot.Add(newContract);
            ContractsRoot.Save(Address + "\\Contracts.xml");
        }

        public void AddMother(Mother m)
        {
            foreach (Nanny ch in GetNannies())
                if (ch.ID == m.ID)
                    throw new Exception("ID already exists!");
            foreach (Child ch in GetChildren())
                if (ch.ID == m.ID)
                    throw new Exception("ID already exists!");
            foreach (Mother ch in GetMothers())
                if (ch.ID == m.ID)
                    throw new Exception("ID already exists!");
            XElement newMother = new XElement("Mother",
                new XElement("ID", m.ID),
                new XElement("FirstName", m.FirstName),
                new XElement("LastName", m.LastName),
                new XElement("Phone", m.Phone),
                new XElement("Address", m.Address),
                new XElement("NeedNannyAddress", m.NeedNannyAddress),
                new XElement("Comments"),
                new XElement("HoursNeeded"),
                new XElement("DaysNeeded"));
            foreach (string comment in m.Comments)
                newMother.Element("Comments").Add(new XElement("Comment", comment));
            for (int i = 0; i < 6; i++)
            {
                newMother.Element("DaysNeeded").Add(new XElement(((Days)i).ToString(), m.DaysNeeded[i]));
                newMother.Element("HoursNeeded").Add(new XElement(((Days)i).ToString(), 
                    new XElement("Begin", m.HoursNeeded[0, i].Hour), new XElement("End", m.HoursNeeded[1,i].Hour)));
            }
            LoadData(TypeToLoad.Mother);
            MothersRoot.Add(newMother);
            MothersRoot.Save(Address + "\\Mothers.xml");
                    
        }

        public void AddNanny(Nanny n)
        {
            foreach (Nanny ch in GetNannies())
                if (ch.ID == n.ID)
                    throw new Exception("ID already exists!");
            foreach (Child ch in GetChildren())
                if (ch.ID == n.ID)
                    throw new Exception("ID already exists!");
            foreach (Mother ch in GetMothers())
                if (ch.ID == n.ID)
                    throw new Exception("ID already exists!");
            XElement newNanny = new XElement("Nanny",
                new XElement("ID", n.ID),
                new XElement("FirstName", n.FirstName),
                new XElement("LastName", n.LastName),
                new XElement("Phone", n.Phone),
                new XElement("Address", n.Address),
                new XElement("Expertise", n.Expertise),
                new XElement("Floor", n.Floor),
                new XElement("HourSalary", n.HourSalary),
                new XElement("MonthSalary", n.MonthSalary),
                new XElement("MaxAge", n.MaxAge),
                new XElement("MinAge", n.MinAge),
                new XElement("MaxChildren", n.MaxChildren),
                new XElement("Elevator", n.Elevator),
                new XElement("IsCostByHour", n.IsCostByHour),
                new XElement("VacationByMinisterOfEducation", n.VacationByMinisterOfEducation),
                new XElement("BirthDate", new XElement("Year", n.BirthDate.Year), new XElement("Month", n.BirthDate.Month), new XElement("Day", n.BirthDate.Day)),
                new XElement("Recommendations"),
                new XElement("WorkDays"),
                new XElement("WorkHours"));
            foreach (string rec in n.Recommendations)
                newNanny.Element("Recommendations").Add(new XElement("Reccomendation", rec));
            for (int i = 0; i < 6; i++)
            {
                newNanny.Element("WorkDays").Add(new XElement(((Days)i).ToString(), n.WorkDays[i]));
                newNanny.Element("WorkHours").Add(new XElement(((Days)i).ToString(),
                    new XElement("Begin", n.WorkHours[0, i].Hour), new XElement("End", n.WorkHours[1, i].Hour)));
            }
            LoadData(TypeToLoad.Nanny);
            NanniesRoot.Add(newNanny);
            NanniesRoot.Save(Address + "\\Nannies.xml");
        }

        public Child FindChildByID(string id)
        {
            foreach (Child c in GetChildren())
                if (c.ID == id)
                    return c;
            return null;
        }

        public Mother FindMotherByID(string id)
        {
            foreach (Mother m in GetMothers())
                if (m.ID == id)
                    return m;
            return null;
        }

        public Nanny FindNannyByID(string id)
        {
            foreach (Nanny n in GetNannies())
                if (n.ID == id)
                    return n;
            return null;
        }

        public List<Child> GetChildren()
        {
            LoadData(TypeToLoad.Child);
            List<Child> retList;
            try
            {
                retList = (from ch in ChildrenRoot.Elements() select new Child(ch.Element("ID").Value, ch.Element("MotherID").Value, ch.Element("FirstName").Value, ch.Element("LastName").Value, (from need in ch.Element("SpecialNeeds").Elements() select need.Value).ToList(), new DateTime(int.Parse(ch.Element("BirthDate").Element("Year").Value), int.Parse(ch.Element("BirthDate").Element("Month").Value), int.Parse(ch.Element("BirthDate").Element("Day").Value)))).ToList();
                foreach(Child c in retList)
                {
                    c.HaveSpecialNeeds = c.SpecialNeeds.Count != 0;
                }
            }
            catch
            {
                retList = new List<Child>();
            }
            return retList;
        }

        public List<Contract> GetContracts()
        {
            LoadData(TypeToLoad.Contract);
            List<Contract> retList;
            try
            {
                retList = (from con in ContractsRoot.Elements() select new Contract() {
                    ChildID = con.Element("ChildID").Value,
                    NannyID = con.Element("NannyID").Value,
                    SerialNumber = con.Element("SerialNumber").Value,
                    PerHour = int.Parse(con.Element("PerHour").Value),
                    PerMonth = int.Parse(con.Element("PerMonth").Value),
                    IsByHour = bool.Parse(con.Element("IsByHour").Value),
                    IsMet = bool.Parse(con.Element("IsMet").Value),
                    IsSigned = bool.Parse(con.Element("IsSigned").Value),
                    Beginning = new DateTime(int.Parse(con.Element("Beginning").Element("Year").Value), int.Parse(con.Element("Beginning").Element("Month").Value), int.Parse(con.Element("Beginning").Element("Day").Value)),
                    End = new DateTime(int.Parse(con.Element("End").Element("Year").Value), int.Parse(con.Element("End").Element("Month").Value), int.Parse(con.Element("End").Element("Day").Value)),

                }).ToList();
            }
            catch
            {
                retList = new List<Contract>();
            }
            return retList;
        }

        public List<Mother> GetMothers()
        {
            LoadData(TypeToLoad.Mother);
            List<Mother> retList;
            try
            {
                retList = (from mom in MothersRoot.Elements()
                           select new Mother()
                           {
                               Address = mom.Element("Address").Value,
                               ID = mom.Element("ID").Value,
                               FirstName = mom.Element("FirstName").Value,
                               LastName = mom.Element("LastName").Value,
                               Phone = mom.Element("Phone").Value,
                               NeedNannyAddress = mom.Element("NeedNannyAddress").Value,
                               //Comments = (from com in mom.Element("Comments").Elements() select com.Value).ToList()
                           }).ToList();               
                foreach (XElement xmom in MothersRoot.Elements())
                {
                    Mother mom = retList.Find(m => m.ID == xmom.Element("ID").Value);
                    foreach (XElement com in xmom.Element("Comments").Elements())
                        mom.Comments.Add(com.Value);
                    foreach (XElement day in xmom.Element("DaysNeeded").Elements())
                    {
                        int i = 0;
                        mom.DaysNeeded[i++] = bool.Parse(day.Value);
                    }
                    foreach (XElement hour in xmom.Element("HoursNeeded").Elements())
                    {
                        int i = 0;
                        mom.HoursNeeded[0, i] = default(DateTime).AddHours(int.Parse(hour.Element("Begin").Value));
                        mom.HoursNeeded[1, i++] = default(DateTime).AddHours(int.Parse(hour.Element("End").Value));
                    }                        
                       
                }
            }
            catch
            {
                retList = new List<Mother>();
            }
            return retList;
        }

        public List<Nanny> GetNannies()
        {
            LoadData(TypeToLoad.Nanny);
            List<Nanny> retList;
            try
            {
                retList = (from nan in NanniesRoot.Elements()
                           select new Nanny()
                           {
                               Address = nan.Element("Address").Value,
                               ID = nan.Element("ID").Value,
                               FirstName = nan.Element("FirstName").Value,
                               LastName = nan.Element("LastName").Value,
                               Phone = nan.Element("Phone").Value,
                               Elevator = bool.Parse(nan.Element("Elevator").Value),
                               IsCostByHour = bool.Parse(nan.Element("IsCostByHour").Value),
                               VacationByMinisterOfEducation = bool.Parse(nan.Element("VacationByMinisterOfEducation").Value),
                               Expertise = int.Parse(nan.Element("Expertise").Value),
                               Floor = int.Parse(nan.Element("Floor").Value),
                               HourSalary = int.Parse(nan.Element("HourSalary").Value),
                               MonthSalary = int.Parse(nan.Element("MonthSalary").Value),
                               MaxAge = int.Parse(nan.Element("MaxAge").Value),
                               MinAge = int.Parse(nan.Element("MinAge").Value),
                               MaxChildren = int.Parse(nan.Element("MaxChildren").Value),
                               BirthDate = new DateTime(int.Parse(nan.Element("BirthDate").Element("Year").Value), int.Parse(nan.Element("BirthDate").Element("Month").Value), int.Parse(nan.Element("BirthDate").Element("Day").Value))

                               //Comments = (from com in mom.Element("Comments").Elements() select com.Value).ToList()
                           }).ToList();
                foreach (XElement xnan in NanniesRoot.Elements())
                {
                    Nanny nan = retList.Find(n => n.ID == xnan.Element("ID").Value);                    
                    foreach (XElement rec in xnan.Element("Recommendations").Elements())
                        nan.Recommendations.Add(rec.Value);
                    foreach (XElement day in xnan.Element("WorkDays").Elements())
                    {
                        int i = 0;
                        nan.WorkDays[i++] = bool.Parse(day.Value);
                    }
                    foreach (XElement hour in xnan.Element("WorkHours").Elements())
                    {
                        int i = 0;
                        nan.WorkHours[0, i] = default(DateTime).AddHours(int.Parse(hour.Element("Begin").Value));
                        nan.WorkHours[1, i++] = default(DateTime).AddHours(int.Parse(hour.Element("End").Value));
                    }
                }
            }
            catch
            {
                retList = new List<Nanny>();
            }
            return retList;
        }

        public void InitSomeVars()
        {
            if (GetNannies().Count > 0 || GetMothers().Count > 0 || GetChildren().Count > 0 || GetContracts().Count > 0)
                return;
            DateTime[,] hours1 = new DateTime[,] { { default(DateTime).AddHours(8), default(DateTime).AddHours(8), default(DateTime).AddHours(8), default(DateTime).AddHours(8), default(DateTime).AddHours(8), default(DateTime).AddHours(8) }, { default(DateTime).AddHours(15), default(DateTime).AddHours(15), default(DateTime).AddHours(19), default(DateTime).AddHours(19), default(DateTime).AddHours(19), default(DateTime).AddHours(19) } },
                hours2 = new DateTime[,] { { default(DateTime).AddHours(15), default(DateTime).AddHours(8), default(DateTime).AddHours(9), default(DateTime).AddHours(10), default(DateTime).AddHours(8), default(DateTime).AddHours(8) }, { default(DateTime).AddHours(23), default(DateTime).AddHours(15), default(DateTime).AddHours(19), default(DateTime).AddHours(18), default(DateTime).AddHours(16), default(DateTime).AddHours(19) } };
            List<string> recs1 = new List<string>(), coms1 = new List<string>();
            coms1.Add("a comment...");
            recs1.Add("very patient");
            recs1.Add("especially good with the youngest ones");
            Nanny n = new Nanny("123", "sara", "levi", "052-000-00-01", "Bialik, Ramat-Gan, Israel", new DateTime(1990, 1, 1), true, true, true, 5, 1, 4, 20, 20, 2000, 3, new bool[] { true, true, true, true, true, false, false }, hours1, recs1);
            Nanny n1 = new Nanny("1234", "rivka", "levi", "052-000-50-01", "Shaanan, Ramat-Gan, Israel", new DateTime(1990, 1, 1), true, false, true, 4, 6, 1, 12, 40, 3000, 3, new bool[] { false, true, true, true, true, true, true }, hours2, recs1);
            Mother m = new Mother("001", "leia", "organa", "058-012-34-56", "Hertzel, Tel-Aviv, Israel", "", new bool[] { true, true, true, true, true, false, false }, hours2, coms1);
            Child c1 = new Child("901", "001", "ruben", "organa", null, new DateTime(2017, 5, 1)),
                c2 = new Child("902", "001", "simon", "organa", null, new DateTime(2017, 5, 5));
            Contract con1 = new Contract(n.ID, c1.ID, false, new DateTime(2018, 1, 1), new DateTime(2018, 3, 1)),
                con2 = new Contract(n.ID, c2.ID, true, new DateTime(2018, 1, 5), new DateTime(2018, 3, 5));
            //SignContract(con2);
            AddNanny(n);
            AddNanny(n1);
            AddMother(m);
            AddChild(c1);
            AddChild(c2);
            AddContract(con1);
            AddContract(con2);
        }

        public void RemoveChild(Child c)
        {
            throw new NotImplementedException();
        }

        public void RemoveContract(Contract c)
        {
            throw new NotImplementedException();
        }

        public void RemoveMother(Mother m)
        {
            throw new NotImplementedException();
        }

        public void RemoveNanny(Nanny n)
        {
            throw new NotImplementedException();
        }

        public void UpdateChild(Child c, Child.Props prop, object newVal)
        {
            throw new NotImplementedException();
        }

        public void UpdateContract(Contract c, Contract.Props prop, object newVal)
        {
            throw new NotImplementedException();
        }

        public void UpdateMother(Mother m, Mother.Props prop, object newVal)
        {
            throw new NotImplementedException();
        }

        public void UpdateNanny(Nanny n, Nanny.Props prop, object newVal)
        {
            throw new NotImplementedException();
        }
    }
}
