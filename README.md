# PCElect
a tool to do some elections (parent council) online 

The Idea is to hand out one QRcode per vote on the parents evening and collect nominees there.
In the following 2 Days eveyone can scan the Code with the Cell and Vote.

For some security it will consist of two parts:
1. a net core trool that 
   - generate Public/Private Keys 
   - generates HTML Pages with QRCodes based on an URL and Crypto Random Number too print out
   - gets encrypted votes, decrypt, compare the Random Number and counts Votes
   - controlls the blockchain
2. an easy web page
   - showing nominees with checkboxes 
   - submitting the vote
   - encrypting the vote and store it (inside a FileSystem)
   - creates an easyblock chain while saveing
   
