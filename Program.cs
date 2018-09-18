using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using static System.Net.WebRequestMethods;

namespace ConsoleApp1
{
    internal class Program
    {
        private const string USERNAME = "Yashashree-Salunke";
        private const string PASSWORD = "yash@1994";

        public static void Main(string[] args)
        {

            UserStories userStories = new UserStories();
            //create clone
            //var co = new CloneOptions();
            //co.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = USERNAME, Password = PASSWORD };
            // Repository.Clone("https://github.com/Yashashree-Salunke/UserStories.git", "C:/Users/admin/Documents/GitHub/UserStories");

            using (var repo = new Repository(@"C:/Users/admin/Documents/GitHub/UserStories"))
            {
                var db = new ReqSpecContextContext();
                DirectoryInfo ParentDirectory = new DirectoryInfo(repo.Info.WorkingDirectory);
                foreach (var u in db.UserStories)
                {
                    if (u.RootId == null && u.ParentId == null)
                    {
                        if (u.HasChildren == true)
                        {

                            var FolderName = u.Title.Replace(" ", "_");
                            var Folder = Directory.CreateDirectory(
                                 Path.Combine(ParentDirectory.FullName, FolderName));

                            string title = u.Title.Replace(" ", "");
                            string fullpath = Path.Combine(repo.Info.WorkingDirectory, title + ".txt");
                            System.IO.File.WriteAllText(fullpath, u.Value);
                            repo.Index.Add(title + ".txt");
                            Commands.Stage(repo, fullpath);

                        }
                        else
                        {
                            string title = u.Title.Replace(" ", "");
                            string fullpath = Path.Combine(ParentDirectory.FullName, title + ".txt");
                            System.IO.File.WriteAllText(fullpath, u.Value);
                            repo.Index.Add(title + ".txt");
                            Commands.Stage(repo, fullpath);

                        }
                    }
                    if (u.ParentId != null)
                    {
                        if (u.HasChildren == true)
                        {
                            CreateFolder();
                        }
                        else
                        {
                            string Filename = u.Title.Replace(" ", "");
                            var title = (from s in db.UserStories where s.Id == u.ParentId select s.Title).Single();
                            List<string> folders = ParentDirectory.GetDirectories(title.Replace(" ", "_"), SearchOption.AllDirectories).Select(p => p.FullName).ToList();
                            string Fullpath = folders.Single();
                            string newpath = Path.Combine(Fullpath, Filename + ".txt");
                            int fileIndex = repo.Info.WorkingDirectory.Count();
                            string file = newpath.Remove(0, fileIndex);
                            System.IO.File.WriteAllText(newpath, u.Value);
                            repo.Index.Add(file);
                            Commands.Stage(repo, newpath);

                        }

                    }

                    void CreateFolder()
                    {
                        List<UserStories> list = new List<UserStories>();

                        string connectionString = "Server=(localdb)\\mssqllocaldb;Database=ReqSpecContext;Trusted_Connection=True;MultipleActiveResultSets=true";
                        SqlConnection con = new SqlConnection(connectionString);
                        SqlCommand command = new SqlCommand("SELECT Id,HasChildren,ParentId,Title FROM UserStories");
                        command.Connection = con;
                        command.Connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            UserStories us = new UserStories();
                            us.Id = (int)reader["Id"];
                            us.HasChildren = (bool)reader["HasChildren"];

                            if (reader["ParentId"] == DBNull.Value)
                            {
                                us.ParentId = 0;
                            }
                            else
                            {
                                us.ParentId = (int)reader["ParentId"];
                            }
                            us.Title = (string)reader["Title"];
                            list.Add(us);

                        }


                        foreach (var item in list)
                        {
                            if (item.ParentId == u.ParentId)
                            {
                                var FolderName = item.Title.Replace(" ", "_");
                                var title = list.Where(x => x.Id == item.ParentId).Select(p => p.Title).Single();
                                List<string> folders = ParentDirectory.GetDirectories(title.Replace(" ", "_"), SearchOption.AllDirectories).Select(p => p.FullName).ToList();
                                string Fullpath = folders.Single();
                                var folder = Directory.CreateDirectory(Path.Combine(Fullpath, FolderName));

                            }

                        }
                    }
                }
                PushAndCommit();
                void PushAndCommit()
                {
                    Signature author = new Signature("Yashashree-Salunke", USERNAME, DateTime.Now);
                    Signature committer = author;
                    Commit commit = repo.Commit("Folder is Created", author, committer);  // Commit to the repository

                    PushOptions options = new PushOptions();
                    options.CredentialsProvider = new CredentialsHandler(
                        (url, usernameFromUrl, types) =>
                            new UsernamePasswordCredentials()
                            {
                                Username = USERNAME,
                                Password = PASSWORD
                            });
                    repo.Network.Push(repo.Branches["master"], options);
                }


            }


        }


    }
}



