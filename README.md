# Crypto

This repo will contain example implementations of cryptographic algorithms developed for learning purposes. They are not intended to be secure or particularly efficient.

## RSA
RSAGenerator includes a C# implementation of the basic RSA algorithm. It does not include any padding method.

### Switches
* /p  - set a manual value for prime p
* /q  - set a manual value for prime q
* /e  - set a manual value for exponent e

If only one of p or q is set manually, then the program will calculate its own values for both inputs.

### Note:
This implementation uses a 'dumb' function to calculate the value of d. Whilst this works fine for smaller numbers (into the thousands) it is prohibitively slow for realistic sized prime inputs. W.I.P. to work out the maths behind a smarted function.
