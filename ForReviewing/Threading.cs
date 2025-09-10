using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForReviewing
{
	internal class Threading
	{
	}

	class Account
	{
		public int Balance { get; private set; }
		private readonly object lockObj = new object();

		public Account(int balance) => Balance = balance;

		public void Withdraw(int amount)
		{
			Balance -= amount;
		}

		public void Deposit(int amount)
		{
			Balance += amount;
		}

		public object GetLock() => lockObj;
	}

	class Program
	{
		// Dead lock
		static void TransferDeadLock(Account from, Account to, int amount)
		{
			lock (from.GetLock())
			{
				Thread.Sleep(100); // long time
				lock (to.GetLock())
				{
					from.Withdraw(amount);
					to.Deposit(amount);
					Console.WriteLine($"Chuyển {amount} từ {from.GetHashCode()} sang {to.GetHashCode()}");
				}
			}
		}

		static void TransferSafe(Account from, Account to, int amount)
		{
			Account first = from.GetHashCode() < to.GetHashCode() ? from : to;
			Account second = from == first ? to : from;

			lock (first.GetLock())
			{
				Thread.Sleep(100);
				lock (second.GetLock())
				{
					from.Withdraw(amount);
					to.Deposit(amount);
					Console.WriteLine($"Move {amount} from {from.GetHashCode()} to {to.GetHashCode()}");
				}
			}
		}

		static void Main()
		{
			var acc1 = new Account(1000);
			var acc2 = new Account(2000);

			Thread t1 = new Thread(() => TransferSafe(acc1, acc2, 100));
			Thread t2 = new Thread(() => TransferSafe(acc2, acc1, 200));

			t1.Start();
			t2.Start();

			t1.Join();
			t2.Join();

			Console.WriteLine($"acc1: {acc1.Balance}, acc2: {acc2.Balance}");
		}
	}
}
