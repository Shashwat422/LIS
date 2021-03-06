﻿using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using DataAccess.Models;
using Windows.UI.Xaml.Controls;

namespace LIS.ViewModels
{
	public class AdminPageViewModel
	{
		private string maxbook = string.Empty;
		private int issuemonth = 0;
		//private bool IsSucceeded = false;
		public string IssueBookStatus { get; set; }
		public string ReturnBookStatus { get; set; }
		
		private async void AddUserCommand(Members user)
		{
			await ApplicationData.Current.LocalFolder.CreateFileAsync("University.db", CreationCollisionOption.OpenIfExists);
			string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "University.db");
			using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
			{
				db.Open();
				try
				{
					String userCommand = "INSERT OR REPLACE INTO users (ID, Name, Password, PhoneNo, BooksIssued, MaxBookLimit, IssueMonthDuration)" +
						" VALUES" +
						$"('{user.MemberId}', '{user.Name}', '{user.Password}'," +
						$"'{user.PhoneNo}', '0', '{maxbook}', '{issuemonth}')";

					SqliteCommand cmd = new SqliteCommand(userCommand, db);

					SqliteDataReader result = cmd.ExecuteReader();

					if (result.RecordsAffected > 0)
						await ShowDialogBox("User Added!");
				}
				catch (Exception e)
				{
					await ShowDialogBox(e.Message);
					//goto closedb;
				}
				db.Close();
			}

		}

		public async void AddUser(Members user, string category)
		{
			if(user.MemberId.Equals("admin", StringComparison.InvariantCultureIgnoreCase))
			{
				await ShowDialogBox("Invalid User Id. Please enter another.");
				return;
			}
			if (string.Equals(category, "Under Grad"))
			{
				maxbook = UnderGrad.MaxBookLimit.ToString();
				issuemonth = UnderGrad.IssueMonthDuration;
			}
			else if (string.Equals(category, "Post Grad"))
			{
				maxbook = PostGrad.MaxBookLimit.ToString();
				issuemonth = PostGrad.IssueMonthDuration;
			}
			else if (string.Equals(category, "Research Scholar"))
			{
				maxbook = ResearchScholar.MaxBookLimit.ToString();
				issuemonth = ResearchScholar.IssueMonthDuration;
			}
			else if (string.Equals(category, "Faculty"))
			{
				maxbook = Faculty.MaxBookLimit.ToString();
				issuemonth = Faculty.IssueMonthDuration;
			}

			AddUserCommand(user);
			
		}

		public async void DeleteUserCommand(string id)
		{
			await ApplicationData.Current.LocalFolder.CreateFileAsync("University.db", CreationCollisionOption.OpenIfExists);
			string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "University.db");
			using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
			{
				db.Open();
				try
				{
					String userCommand = "DELETE FROM users" +
						$" WHERE ID = '{id}'";

					SqliteCommand cmd = new SqliteCommand(userCommand, db);

					SqliteDataReader result = cmd.ExecuteReader();

					if (result.RecordsAffected > 0)
						await ShowDialogBox("User Deleted!");
				}
				catch (Exception e)
				{
					await ShowDialogBox(e.Message);
					//goto closedb;
				}
				db.Close();
			}
		}
		
		public async void AddBookCommand(Book book)
		{
			await ApplicationData.Current.LocalFolder.CreateFileAsync("University.db", CreationCollisionOption.OpenIfExists);
			string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "University.db");
			using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
			{
				db.Open();
				try
				{
					String userCommand = "INSERT OR REPLACE INTO books (BookID, Name, Author, Price, RackNo, Status," +
						" Edition, Category, IssuedTo, IsReserved, ReservedTo, IssueCount)" +
						" VALUES" +
						$"('{book.BookId}', '{book.Name}', '{book.Author}', '{book.Price}', '{book.RackNo}', " +
						$"'{book.Status}', '{book.Edition}', '{book.Category}', 'None', 'No', 'None', 0)";

					SqliteCommand cmd = new SqliteCommand(userCommand, db);

					SqliteDataReader result = cmd.ExecuteReader();

					if (result.RecordsAffected > 0)
						await ShowDialogBox("Book added successfully");
				
				}
				catch (Exception e)
				{
					IssueBookStatus = e.Message;
					//await ShowDialogBox(e.Message);
					//goto closedb;
				}
				db.Close();
			}
		}

		public async void DeleteBookCommand(string bookid)
		{
			await ApplicationData.Current.LocalFolder.CreateFileAsync("University.db", CreationCollisionOption.OpenIfExists);
			string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "University.db");
			using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
			{
				db.Open();

				String userCommand = "DELETE FROM books" +
					$" WHERE BookID = '{bookid}'";

				SqliteCommand cmd = new SqliteCommand(userCommand, db);

				SqliteDataReader result = cmd.ExecuteReader();

				if (result.RecordsAffected > 0)
					await ShowDialogBox("Successfully Deleted");
				db.Close();
			}
		}

		//public string IssueBook(string bookid, string memid)
		//{
		//	IssueBookCommand(bookid, memid);
		//	return IssueBookStatus;
		//}

		public async void IssueBookCommand(string bookid, string memid)
		{
			int booksissueed = 0, maxbooklimit = 0, issueCount = 0;
			string bookstatus = string.Empty, reservestatus = string.Empty;
			string rackno = string.Empty, reservedto = string.Empty;
			await ApplicationData.Current.LocalFolder.CreateFileAsync("University.db", CreationCollisionOption.OpenIfExists);
			string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "University.db");
			using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
			{
				db.Open();

				String GetUserCommand = "Select BooksIssued, MaxBookLimit from users" +
					$" WHERE ID = '{memid}'";
				try
				{
					SqliteCommand usercmd = new SqliteCommand(GetUserCommand, db);

					SqliteDataReader UserResult = usercmd.ExecuteReader();
					while(UserResult.Read())
					{
						booksissueed = int.Parse(UserResult.GetString(0));
						maxbooklimit = int.Parse(UserResult.GetString(1));
					}
					if(maxbooklimit - booksissueed >0)
					{
					
							string BookStatusCommand = "Select Status, IsReserved, IssueCount, RackNo, ReservedTo from books" +
								$" WHERE BookID = '{bookid}'";
							SqliteCommand bookcmd = new SqliteCommand(BookStatusCommand, db);
							SqliteDataReader BookResult = bookcmd.ExecuteReader();

							while (BookResult.Read())
							{
								bookstatus = BookResult.GetString(0);
								reservestatus = BookResult.GetString(1);
								issueCount = int.Parse(BookResult.GetString(2));
								rackno = BookResult.GetString(3);
								if (string.Equals(reservestatus, "Yes", StringComparison.InvariantCultureIgnoreCase))
									reservedto = BookResult.GetString(4);
						}
						//if book available and not reserved then issue
						if(string.Equals(bookstatus, "Available", StringComparison.InvariantCultureIgnoreCase) 
							&& string.Equals(reservestatus, "No", StringComparison.InvariantCultureIgnoreCase))
						{
							string issuebookquery = $"UPDATE books SET Status = 'Issued', IssuedTo = '{memid}'," +
								$" IssueDate = datetime('now','localtime'), IssueCount = {issueCount+1}" +
								$" WHERE BookID = '{bookid}'";
							SqliteCommand issuebookcmd = new SqliteCommand(issuebookquery, db);
							SqliteDataReader IssueResult = issuebookcmd.ExecuteReader();

							string incuserbooknoquery = $"UPDATE users SET BooksIssued = {booksissueed+1} " +
								$" WHERE ID = '{memid}'";
							SqliteCommand incuserbookcmd = new SqliteCommand(incuserbooknoquery, db);
							SqliteDataReader IncMembookResult = incuserbookcmd.ExecuteReader();

							if (IssueResult.RecordsAffected > 0 && IncMembookResult.RecordsAffected > 0)
								await ShowDialogBox( $"Successfully Issued\n Rack No: {rackno}");
							else
								await ShowDialogBox("Unable to issue book.");
						}

						//if book available and reserved then issue to reserved member
						else if (string.Equals(bookstatus, "Available", StringComparison.InvariantCultureIgnoreCase)
							&& string.Equals(reservestatus, "Yes", StringComparison.InvariantCultureIgnoreCase)
							&& reservedto.Equals(memid,StringComparison.InvariantCultureIgnoreCase))
						{
							string issuebookquery = $"UPDATE books SET Status = 'Issued', IssuedTo = '{memid}'," +
								$" IssueDate = datetime('now','localtime'), IssueCount = {issueCount + 1}, " +
								$"IsReserved = 'No', ReservedTo = 'None'" +
								$" WHERE BookID = '{bookid}'";
							SqliteCommand issuebookcmd = new SqliteCommand(issuebookquery, db);
							SqliteDataReader IssueResult = issuebookcmd.ExecuteReader();

							string incuserbooknoquery = $"UPDATE users SET BooksIssued = {booksissueed + 1} " +
								$" WHERE ID = '{memid}'";
							SqliteCommand incuserbookcmd = new SqliteCommand(incuserbooknoquery, db);
							SqliteDataReader IncMembookResult = incuserbookcmd.ExecuteReader();

							if (IssueResult.RecordsAffected > 0 && IncMembookResult.RecordsAffected > 0)
								await ShowDialogBox($"Successfully Issued\n Rack No: {rackno}");
							else
								await ShowDialogBox("Unable to issue book.");
						}

						else
						{
							await ShowDialogBox("Sorry, the Book is Issued or Reserved.");
						}
					}
					else
					{
						await ShowDialogBox("Max Book Limit Exceed. Return a book and try again.");
					}
				}
					catch (Exception e)
				{
					await ShowDialogBox(e.Message);
					goto closedb;
				}
			closedb:
				db.Close();
			}
		}

		private async Task ShowDialogBox(string content)
		{
			ContentDialog ResultDialog = new ContentDialog
			{
				Title = "Message",
				Content = content,
				CloseButtonText = "Got It!"
			};

			await ResultDialog.ShowAsync();
		}

		public string ReturnBook(string bookid)
		{
			ReturnBookCommand(bookid);
			return ReturnBookStatus;
		}

		public async void ReturnBookCommand(string bookid)
		{
			int booksissueed = 0, issueduration = 0;
			double days = 0.0, fine = 0.0;
			string bookstatus = string.Empty, IssuedMem = string.Empty;
			await ApplicationData.Current.LocalFolder.CreateFileAsync("University.db", CreationCollisionOption.OpenIfExists);
			string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "University.db");
			using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
			{
				db.Open();
				string BookStatusCommand = "Select Status, IssuedTo, (julianday() - julianday(IssueDate, 'utc')) from books" +
						$" WHERE BookID = '{bookid}'";
				try
				{
					SqliteCommand bookcmd = new SqliteCommand(BookStatusCommand, db);
					SqliteDataReader BookResult = bookcmd.ExecuteReader();
				
					while (BookResult.Read())
					{
						bookstatus = BookResult.GetString(0);
					}
				
					if (string.Equals(bookstatus, "Issued"))
					{
						while (BookResult.Read())
						{
							IssuedMem = BookResult.GetString(1);
							days = double.Parse(BookResult.GetString(2));
						}
						String GetUserCommand = "Select BooksIssued, IssueMonthDuration from users" +
						$" WHERE ID = '{IssuedMem}'";

						SqliteCommand usercmd = new SqliteCommand(GetUserCommand, db);

						SqliteDataReader UserResult = usercmd.ExecuteReader();
						while (UserResult.Read())
						{
							booksissueed = int.Parse(UserResult.GetString(0));
							issueduration = int.Parse(UserResult.GetString(1));
						}
						if((issueduration*30) - days < 0)
						{
							fine = CalculateFine(days - issueduration);
							ContentDialog FineDialog = new ContentDialog
							{
								Title = $"Collect Fine Rs.{fine}",
								Content = "Close only aftercollecting fine",
								PrimaryButtonText = "Fine Paid",
								SecondaryButtonText = "Fine not Paid"

							};
							ContentDialogResult res = await FineDialog.ShowAsync();
							if(res == ContentDialogResult.Primary)
							{
								goto returnprocess;
								
							}
							else if(res == ContentDialogResult.Secondary)
							{
								await ShowDialogBox("Fine not paid. Book is not returned.");
								goto dbclose;
							}
						}
						returnprocess:

						CreateBill(bookid, IssuedMem, fine);
						string returnbookquery = $"UPDATE books SET Status = 'Available', IssuedTo = 'None'," +
						" IssueDate = 'None'" +
						$" WHERE BookID = '{bookid}'";
						SqliteCommand returnbookcmd = new SqliteCommand(returnbookquery, db);
						SqliteDataReader ReturnResult = returnbookcmd.ExecuteReader();

						string decuserbooknoquery = $"UPDATE users SET BooksIssued = {booksissueed - 1} " +
							$" WHERE ID = '{IssuedMem}'";
						SqliteCommand decuserbookcmd = new SqliteCommand(decuserbooknoquery, db);
						SqliteDataReader DecMembookResult = decuserbookcmd.ExecuteReader();

						if (ReturnResult.RecordsAffected>0 && DecMembookResult.RecordsAffected>0)
						{
							await ShowDialogBox("Book Returned");							
						}
						else
						{
							await ShowDialogBox("Book Not Returned");
						}
					}
					else
						await ShowDialogBox("Book is not issued to anyone.");
				}
				catch (Exception e)
				{
					await ShowDialogBox($"Book Not Issued \n{e.Message}");
					goto dbclose;
				}


			dbclose:
				db.Close();
			}
		}

		private void CreateBill(string bookid, string issuedMem, double fine)
		{
			
		}

		public double CalculateFine(double days)
		{
			//double fine = 0.0;

			return days*Bill.PenaltyRate;
		}
	}
}
