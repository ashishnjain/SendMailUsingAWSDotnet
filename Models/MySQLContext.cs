using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendMailforAWSorSMTP.Models
{
    public class MySQLContext
    {
        public string ConnectionString { get; set; }

        public MySQLContext(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
        public List<EmailLogs> GetEmailLogs()
        {
            List<EmailLogs> emailLogs = new List<EmailLogs>();
            try
            {
                using(MySqlConnection conn = GetConnection())
                {
                    if (conn.State == System.Data.ConnectionState.Closed)
                        conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT max(sentOn) as senton,count(id) as counts,campaign FROM t_emaillogs group by campaign order by 1 desc limit 1000", conn);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            emailLogs.Add(new EmailLogs()
                            {
                                //deliveredOn = Convert.ToDateTime(reader["deliveredOn"]),
                                Email = reader["counts"].ToString(),
                                //isDelivered = Convert.ToBoolean(reader["isDelivered"]),
                                Message = reader["campaign"].ToString(),
                                // MessageID = reader["messageId"].ToString(),
                                // Name = reader["name"].ToString(),
                                // replyTo = reader["replyTo"].ToString(),
                                sentOn = Convert.ToDateTime(reader["senton"])
                            });
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw;
            }
            return emailLogs;
        }
        public bool SaveEmailLog(EmailLogs model)
        {
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    if (conn.State == System.Data.ConnectionState.Closed)
                        conn.Open();
                    string sqlcommand = "insert into t_emaillogs(name,email,subject,message,messageID,replyTo,sentOn, campaign) values('"
                        + model.Name + "','" + model.Email + "','" + model.Subject + "','" + model.Message+
                        "','" + model.MessageID + "','" + model.replyTo + "','" + model.sentOn.Value.ToString("yyyy-MM-dd HH:mm:ss") + "','"+model.Campaign+"')";
                    MySqlCommand cmd = new MySqlCommand(sqlcommand, conn);
                    cmd.ExecuteNonQuery();
                }

            }
            catch(Exception ex)
            {
                throw;
            }
            return true;
        }
        public List<EmailLogs> GetCampaignLogs(string campaign)
        {
            List<EmailLogs> emailLogs = new List<EmailLogs>();
            try
            {
                using(MySqlConnection conn = GetConnection())
                {
                    if (conn.State == System.Data.ConnectionState.Closed)
                        conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT name,email,senton,messageId,replyTo FROM t_emaillogs where campaign='"+campaign+"' order by id desc ", conn);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            emailLogs.Add(new EmailLogs()
                            {
                                //deliveredOn = Convert.ToDateTime(reader["deliveredOn"]),
                                Email = reader["email"].ToString(),
                                //isDelivered = Convert.ToBoolean(reader["isDelivered"]),
                                Message = campaign,
                                MessageID = reader["messageId"].ToString(),
                                Name = reader["name"].ToString(),
                                replyTo = reader["replyTo"].ToString(),
                                sentOn = Convert.ToDateTime(reader["senton"])
                            });
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw;
            }
            return emailLogs;
        }
    }
}
