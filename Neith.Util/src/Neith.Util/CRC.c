//******************************************************************************
// short ComputeCrc16 -- Compute 16 bit CRC
//
// This static method calculates the the 16 bit CRC.  The broadcast
// packets use a 16 bit cyclic redundancy checksum (CRC) using a
// polynomial of the form X5 + X. The packet excluding trailing
// CRC-code must be processed through this routine.
//
//******************************************************************************
#define POLY_HIBIT (1L<<16)
#define POLY_MASK (POLY_HIBIT-1)
#define POLY_NOME (POLY_HIBIT+(1<<5)+1)
short ComputeCrc16( const void * pktIP, // Ptr to the packet start
                    int lenI            // Len to be processed thru char preceeding CRC
                  )
{
    int crc = 0, val, byteX, bitX;
    const unsigned char * pktP = (const unsigned char *) pktIP;

    for ( byteX=0; byteX < lenI; byteX++ )
    {

        val = pktP[byteX];

        for (bitX=0; bitX<8; bitX++)
        {
            crc = (crc<<1) | (val & 1);
            val >>= 1;

            if (crc & POLY_HIBIT) {
                crc ^= POLY_NOME;
            }
        }
    }
    return (short)(crc & POLY_MASK);

}
/*

��̏������Wbit�P�ʂɕϊ�����Ɓd�d

�v�Z�O��CRC [CH][CL]�i���8bit / ����8bit�j
���͒l  B

���Z�F[CRC�̉��ʂWbit][���͒l�̃r�b�g����t�]�������l]

���Z�l�ɁA[CRC���8bit���狁�߂�bit���]��]��XOR

��bit���]�̔z��ƁAXOR�z������ꂼ��쐬���ė��p

*/
