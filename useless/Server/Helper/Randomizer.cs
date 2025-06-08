using System;
using System.Text;

namespace Server.Helper
{
	// Token: 0x02000074 RID: 116
	internal class Randomizer
	{
		// Token: 0x17000024 RID: 36
		// (get) Token: 0x06000270 RID: 624 RVA: 0x00023B6C File Offset: 0x00021D6C
		// (set) Token: 0x06000271 RID: 625 RVA: 0x00023B73 File Offset: 0x00021D73
		public static Random random { get; private set; } = new Random();

		// Token: 0x06000272 RID: 626 RVA: 0x00023B7B File Offset: 0x00021D7B
		public static string getRandomCharacters()
		{
			return Randomizer.getRandomCharacters(Randomizer.random.Next(6, 32));
		}

		// Token: 0x06000273 RID: 627 RVA: 0x00023B90 File Offset: 0x00021D90
		public static string getRandomCharacters(int count)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 1; i <= count; i++)
			{
				int index = Randomizer.random.Next(0, "asdfghjklqwertyuiopmnbvcxz123456890+_)(*&^%$#@!".Length);
				stringBuilder.Append("asdfghjklqwertyuiopmnbvcxz123456790+_)(*&^%$#@!"[index]);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000274 RID: 628 RVA: 0x00023BDD File Offset: 0x00021DDD
		public static string getRandomCharactersAscii()
		{
			return Randomizer.getRandomCharactersAscii(Randomizer.random.Next(6, 32));
		}

		// Token: 0x06000275 RID: 629 RVA: 0x00023BF4 File Offset: 0x00021DF4
		public static string getRandomCharactersAscii(int count)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 1; i <= count; i++)
			{
				int index = Randomizer.random.Next(0, "asdfghjklqwertyuiopmnbvcxz123456890".Length);
				stringBuilder.Append("asdfghjklqwertyuiopmnbvcxz123456790"[index]);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000276 RID: 630 RVA: 0x00023C44 File Offset: 0x00021E44
		public static string Shuffle(string str)
		{
			char[] array = str.ToCharArray();
			Random random = new Random();
			int i = array.Length;
			while (i > 1)
			{
				i--;
				int num = random.Next(i + 1);
				char c = array[num];
				array[num] = array[i];
				array[i] = c;
			}
			return new string(array);
		}

		// Token: 0x04000177 RID: 375
		public static string[] LegalNaming = new string[]
		{
			"Guna",
			"MetroFramework"
		};
	}
}
