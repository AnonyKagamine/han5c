using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Collections;

namespace han5c.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;

		private Hashtable ht=new Hashtable();

		[TempData]
		public string MyMessage{get;set;}
		[TempData]
		public string MyContent{get;set;}
		[TempData]
		public string MyKey{get;set;}

		static string szHan5="，的一是了我不人在他有这个上们来到时大地为子中你说生国年着就那和要她出也得里后以会家可下而过天去能对小多然于心学么之都好看起发当没成只如事把还用第样道想作种开美总从无情己面最女但现前些所同日手又行意动方期它头经长儿回位分爱老因很给名法间斯知世什两次使身者被高已亲其进此话常与活正感见明问力理尔点文几定本公特做外孩相西果走将月十实向声车全信重三机工物气每并别真打太新比才便夫再书部水像眼等体却加电主界门利海受听表德少克代员许先口由死安写性马光白或住难望教命花结乐色更拉东神记处让母父应直字场平报友关放至张认接告入笑";

		public IndexModel(ILogger<IndexModel> logger)
		{
			_logger = logger;

			for(int i=0;i<szHan5.Length;i++)
				ht.Add(szHan5[i], (byte)i);
		}

		public void OnGet()
		{

		}

		public IActionResult OnPostEncode(String inputKey, String inputCode)
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			MyContent=EncryptStringToHan5c(inputCode, inputKey);
			return RedirectToPage("./Index");
		}

		public IActionResult OnPostDecode(String inputKey, String inputCode)
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			MyContent=DecryptStringFromHan5c(inputCode, inputKey);
			return RedirectToPage("./Index");
		}

		string EncryptStringToHan5c(String han5c, String han5k)
		{
			if(string.IsNullOrEmpty(han5c)){
				MyMessage="输入异常";
				return "没有编码内容?";}
			if(string.IsNullOrEmpty(han5k)){
				MyMessage="输入异常";
				return "没有密钥?";}

			byte[] bsKey=new byte[32];
			if(Encoding.UTF8.GetBytes(han5k).Length<32){
				MyMessage="输入异常";
				return "密钥长度无效（密钥长度须不小于16个汉字）";}
			Array.Copy(Encoding.UTF8.GetBytes(han5k),0,bsKey,0,32);
			MyKey=han5k;

			byte[] bsResult=EncryptStringToBytes(han5c, bsKey);
			string strResult="";
			for(int i=0;i<bsResult.Length;i++)
				strResult+=szHan5[bsResult[i]];
			MyMessage="";
			return strResult;
		}

		static byte[] EncryptStringToBytes(string plainText, byte[] Key)
		{
			// Check arguments.
			if (plainText == null || plainText.Length <= 0)
				throw new ArgumentNullException("plainText");
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException("Key");
			// Result list
			List<byte> liRes=new List<byte>();

			// Create an RijndaelManaged object
			// with the specified key.
			using (RijndaelManaged rijAlg = new RijndaelManaged())
			{
				rijAlg.Key = Key;

				// Create an encryptor to perform the stream transform.
				ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

				// Create the streams used for encryption.
				using (MemoryStream msEncrypt = new MemoryStream())
				{
					using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
					{
						using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
						{

							//Write all data to the stream.
							swEncrypt.Write(plainText);
						}
						foreach(byte t in rijAlg.IV)liRes.Add(t);
						byte[] enc=msEncrypt.ToArray();
						foreach(byte t in enc)liRes.Add(t);
					}
				}
			}

			// Return the encrypted bytes from the memory stream.
			return liRes.ToArray();
		}

		string DecryptStringFromHan5c(String han5c, String han5k)
		{
			if(string.IsNullOrEmpty(han5c)){
				MyMessage="输入异常";
				return "没有编码内容?";}
			if(string.IsNullOrEmpty(han5k)){
				MyMessage="输入异常";
				return "没有密钥?";}

			byte[] bsKey=new byte[32];
			if(Encoding.UTF8.GetBytes(han5k).Length<32){
				MyMessage="输入异常";
				return "密钥长度无效（密钥长度须不小于16个汉字）";}
			Array.Copy(Encoding.UTF8.GetBytes(han5k),0,bsKey,0,32);
			MyKey=han5k;

			List<byte> liCode=new List<byte>();
			for(int i=0;i<han5c.Length;i++){
				if(!ht.ContainsKey(han5c[i]))continue;
				liCode.Add((byte)ht[han5c[i]]);
			}
			byte[] bsCode=liCode.ToArray();
			byte[] bsIV=new byte[16];
			Array.Copy(bsCode,0,bsIV,0,16);
			byte[] bsContent=new byte[bsCode.Length-16];
			Array.Copy(bsCode,16,bsContent,0,bsCode.Length-16);
			try{
				MyMessage="";
				return DecryptStringFromBytes(bsContent,bsKey,bsIV);}
			catch(CryptographicException e){
				MyMessage="解码失败";
				return e.Message;}
		}

		static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
		{
			// Check arguments.
			if (cipherText == null || cipherText.Length <= 0)
				throw new ArgumentNullException("cipherText");
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException("Key");
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException("IV");

			// Declare the string used to hold
			// the decrypted text.
			string plaintext = null;

			// Create an RijndaelManaged object
			// with the specified key and IV.
			using (RijndaelManaged rijAlg = new RijndaelManaged())
			{
				rijAlg.Key = Key;
				rijAlg.IV = IV;

				// Create a decryptor to perform the stream transform.
				ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

				// Create the streams used for decryption.
				using (MemoryStream msDecrypt = new MemoryStream(cipherText))
				{
					using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
					{
						using (StreamReader srDecrypt = new StreamReader(csDecrypt))
						{
							// Read the decrypted bytes from the decrypting stream
							// and place them in a string.
							plaintext = srDecrypt.ReadToEnd();
						}
					}
				}
			}

			return plaintext;
		}		
	}
}
