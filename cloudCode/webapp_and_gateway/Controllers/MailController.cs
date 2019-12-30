// ------------------------------------------------------------------------------
// <copyright file="MailController.cs">
//  This controller provides the UI and methods for users to view and classify their mail
// </copyright>
// ------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TrainMailEdge.Controllers
{
    using System.Data.SqlClient;
    using Models;

    /// <summary>
    /// This controller provides a simple UI for users to view their mail and classify as spam/not spam
    /// </summary>
    public class MailController : Controller
    {
        private string cString = "inert_db_connection_here";

        // GET: Mail
        public ActionResult Index()
        {
            try
            {
                var list = GetItems();
                return View(list.OrderByDescending(s=>s.Id));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
           
           
        }

       
        // GET: Mail/Edit/5
        public ActionResult Edit(int id)
        {
            var model = new MailItem();
            return View(model);
        }

        // POST: Mail/Edit/5. Edit whether a mailitem is spam or not
        [HttpPost]
        public ActionResult Edit(int id, MailItem mailItem)
        {
            try
            {
                var isSpam = mailItem.IsSpam;
                var usercategory = isSpam ? 1 : 0;
                using (SqlConnection c = new SqlConnection(cString))
                {
                    c.Open();
                    using (SqlCommand cmd =
                        new SqlCommand(
                            $"update mail set usercategory={usercategory} where mailId={id}",
                            c))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                return Index();
            }
        }

        /// <summary>
        /// Query to get all mail items from the DB
        /// </summary>
        /// <returns></returns>
        public List<MailItem> GetItems()
        {
            var list = new List<MailItem>();
            using (SqlConnection c = new SqlConnection(cString))
            {
                c.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT mailid, receiveddate, fromuser, touser, sentdate, mlcategory, usercategory FROM mail", c))
                {
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var id = rdr.GetInt32(0);
                            var receiveddate = rdr.IsDBNull(1) ? "" : rdr.GetDateTime(1).ToLongDateString();
                            var sender = rdr.GetInt32(2);
                            var receiver = rdr.GetInt32(3);
                            var senddate = rdr.GetDateTime(4).ToLongDateString();
                            var predictedSpam = rdr.GetInt32(5) != 0;
                            var isspam = rdr.GetInt32(6) != 0;
                            list.Add(new MailItem()
                            {
                                Id = id,
                                ReceiveDate = receiveddate,
                                Sender = GetUserName(sender),
                                Receiver = GetUserName(receiver),
                                SendDate = senddate,
                                PredictedSpam = predictedSpam,
                                IsSpam = isspam
                            });
                        }
                    }
                }
            }


            return list;

        }

        /// <summary>
        /// Gets the username for the given userId
        /// </summary>
        /// <param name="id">UserId</param>
        /// <returns></returns>
        private string GetUserName(int id)
        {
            string name = string.Empty;
            using (SqlConnection c = new SqlConnection(cString))
            {
                c.Open();
                using (SqlCommand cmd = new SqlCommand($"SELECT username FROM usertable where userid={id}", c))
                {
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            name = rdr.GetString(0);
                            return name;
                        }
                    }
                }
            }

            return name;
        }
    }
}
