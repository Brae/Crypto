using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Numerics;
using System.Diagnostics;

namespace RSAGenerator {
    struct CalculatingMMI {
        public BigInteger a, b, c, d;
    }

    class RSAGen {
        private int keyLength;
        private BigInteger primeP = 0, primeQ = 0, keyE = 0, keyD = 0;
        private BigInteger valN, valTotient;

        private void parseArgs(string[] args) {
            for (int i = 0; i < args.Length; i++) {
                switch (args[i]) {
                    case "/s": // size of private key
                        i++;
                        keyLength = Int32.Parse(args[i]);
                        break;
                    case "/p":
                        i++;
                        primeP = Int32.Parse(args[i]);
                        break;
                    case "/q":
                        i++;
                        primeQ = Int32.Parse(args[i]);
                        break;
                    case "/e":
                        i++;
                        keyE = Int32.Parse(args[i]);
                        break;
                    default:
                        Console.WriteLine("Error: Unknown argument '" + args[i] + "'");
                        break;

                }
            }
        }

        private BigInteger getN(BigInteger p, BigInteger q) {
            return BigInteger.Multiply(p, q);
        }

        private BigInteger getTotient(BigInteger p, BigInteger q) {
            return BigInteger.Multiply(p - 1, q - 1);
        }

        private BigInteger getRandomPrimeSSL() {
            Boolean prime = false;
            byte[] strOutput = new byte[64];
            RNGCryptoServiceProvider rngCrypto = new RNGCryptoServiceProvider();
            do {
                rngCrypto.GetBytes(strOutput);
                BigInteger intOutput = new BigInteger(strOutput);

                if (intOutput.Sign == -1) {
                    continue;
                }

                Process pProcess2 = new Process();
                pProcess2.StartInfo.FileName = "C:/Users/Peter/Desktop/VMSHARE/Visual Studio Projects/RSAGenerator/RSAGenerator/openssl.exe";
                pProcess2.StartInfo.Arguments = "prime " + intOutput;
                pProcess2.StartInfo.UseShellExecute = false;
                pProcess2.StartInfo.RedirectStandardOutput = true;
                pProcess2.StartInfo.RedirectStandardError = true;
                pProcess2.Start();
                string strPrime = pProcess2.StandardOutput.ReadToEnd();
                pProcess2.WaitForExit();

                string[] strPrimeWords = strPrime.Split(' ');
                if (strPrimeWords.Length == 4) {
                    Console.WriteLine("Prime test: " + strPrimeWords[0] + " " + strPrimeWords[1] + " " + strPrimeWords[2] + " " + strPrimeWords[3]);
                    prime = false;
                } else if (strPrimeWords.Length == 3) {
                    prime = true;
                }
            } while (!prime);
            return new BigInteger(strOutput);
        }

        private BigInteger getE(BigInteger t) {
            RNGCryptoServiceProvider rngProv = new RNGCryptoServiceProvider();
            Boolean primeE = false;
            byte[] rndE = new byte[1];
            do {
                Random rnd = new Random();
                int size = rnd.Next(t.ToByteArray().Length / 2, t.ToByteArray().Length);
                rndE = new byte[size];
                rngProv.GetBytes(rndE);
                if (BigInteger.Compare(new BigInteger(rndE), t) < 0) {
                    Process pProcess = new Process();
                    pProcess.StartInfo.FileName = "C:/Users/Peter/Desktop/VMSHARE/Visual Studio Projects/RSAGenerator/RSAGenerator/openssl.exe";
                    pProcess.StartInfo.Arguments = "prime " + new BigInteger(rndE);
                    pProcess.StartInfo.UseShellExecute = false;
                    pProcess.StartInfo.RedirectStandardOutput = true;
                    pProcess.StartInfo.RedirectStandardError = true;
                    pProcess.Start();
                    string strPrime = pProcess.StandardOutput.ReadToEnd();
                    pProcess.WaitForExit();

                    string[] strPrimeWords = strPrime.Split(' ');
                    if (strPrimeWords.Length == 4) {
                        Console.Write("\rCalculating e...");
                        primeE = false;
                    } else if (strPrimeWords.Length == 3) {
                        Console.WriteLine();
                        primeE = true;
                    }
                }                
            } while (!primeE);
            return new BigInteger(rndE);
        }


        //TODO - incomplete. Need to work out how to do this bit
        private BigInteger getD(BigInteger n, BigInteger e) {
            BigInteger a = n, b = 0, c = e, d = 0;
            List<CalculatingMMI> records = new List<CalculatingMMI>();
            
            do {
                b = BigInteger.Divide(a, c);
                d = BigInteger.Remainder(a, c);
                CalculatingMMI temp = new CalculatingMMI();
                temp.a = a;
                temp.b = b;
                temp.c = c;
                temp.d = d;
                records.Add(temp);
            } while (d != 1);
            return new BigInteger(0);
        }

        static void Main(string[] args) {
            RSAGen program = new RSAGen();
            program.parseArgs(args);

            if (program.primeP == 0 && program.primeQ == 0) {
                program.primeP = program.getRandomPrimeSSL();                
                program.primeQ = program.getRandomPrimeSSL();
            }

            Console.WriteLine("\nPrime P: " + program.primeP + "\nPrime Q: " + program.primeQ);

            // n=pq
            program.valN = program.getN(program.primeP, program.primeQ);
            Console.WriteLine("Modulus n: " + program.valN);

            // Totient function
            program.valTotient = program.getTotient(program.primeP, program.primeQ);
            Console.WriteLine("Totient value: " + program.valTotient);

            // Choose e (skip for now as using known value for example purposes. Add if time to do so
            if (program.keyE == 0) {
                program.keyE = program.getE(program.valTotient);
            }
            Console.WriteLine("Key e: " + program.keyE);

            // Calculate final formula for 0 < d < totient
            // Temporary solution - to be replaced by Modular Multiplicative Inverse function
            for (BigInteger d = 1; d < program.valTotient; d++) {
                BigInteger result = (d * program.keyE) % program.valTotient;
                Console.Write("\rd = " + d + " :: r = " + result + "\t\t\t");
                if (result == 1) {
                    Console.WriteLine("\nKey d: " + d);
                    program.keyD = d;
                    break;
                }
            }

            //program.keyD = BigInteger.ModPow(program.keyE, 1, program.valTotient); TODO - probably needs to be deleted but keep until the getD function is complete


            if (program.keyD == 0) {
                Console.WriteLine("Failed to find value for d with current exponent");
            }

            Console.WriteLine("Public Key: ({0},{1})\n\nPrivate Key: ({2})", program.valN, program.keyE, program.keyD);
            Console.WriteLine("Done");
            //Console.ReadLine();
        }
    }
}
