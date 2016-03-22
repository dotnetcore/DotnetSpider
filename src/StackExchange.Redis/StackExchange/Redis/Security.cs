namespace System.Net.Security
{
       using System.Security.Cryptography.X509Certificates;
       using  System.Net.Security;
    
       public delegate  X509Certificate LocalCertificateSelectionCallback(object sender, string targetHost,  X509CertificateCollection localCertificates,  X509Certificate remoteCertificate, string[] acceptableIssuers);
        
       public delegate bool RemoteCertificateValidationCallback(object sender,  X509Certificate certificate,  X509Chain chain, SslPolicyErrors sslPolicyErrors);
       
    public abstract class AuthenticatedStream : System.IO.Stream
    {
        protected AuthenticatedStream(System.IO.Stream innerStream, bool leaveInnerStreamOpen) { }
        public abstract bool IsAuthenticated { get; }
        public abstract bool IsEncrypted { get; }
        public abstract bool IsMutuallyAuthenticated { get; }
        public abstract bool IsServer { get; }
        public abstract bool IsSigned { get; }
        public bool LeaveInnerStreamOpen { get { return default(bool); } }
        protected System.IO.Stream InnerStream { get { return default(System.IO.Stream); } }
        protected override void Dispose(bool disposing) { }
    }
    
     public partial class SslStream : AuthenticatedStream
    {
        public SslStream(System.IO.Stream innerStream) : base(innerStream, false) { }
        public SslStream(System.IO.Stream innerStream, bool leaveInnerStreamOpen) : base(innerStream, leaveInnerStreamOpen) { }
        public SslStream(System.IO.Stream innerStream, bool leaveInnerStreamOpen,  RemoteCertificateValidationCallback userCertificateValidationCallback) : base(innerStream, leaveInnerStreamOpen) { }
        public SslStream(System.IO.Stream innerStream, bool leaveInnerStreamOpen,  RemoteCertificateValidationCallback userCertificateValidationCallback,  LocalCertificateSelectionCallback userCertificateSelectionCallback) : base(innerStream, leaveInnerStreamOpen) { }
        public SslStream(System.IO.Stream innerStream, bool leaveInnerStreamOpen,  RemoteCertificateValidationCallback userCertificateValidationCallback,  LocalCertificateSelectionCallback userCertificateSelectionCallback, System.Net.Security.EncryptionPolicy encryptionPolicy) : base(innerStream, leaveInnerStreamOpen) { }
        public override bool CanRead { get { return default(bool); } }
        public override bool CanSeek { get { return default(bool); } }
        public override bool CanTimeout { get { return default(bool); } }
        public override bool CanWrite { get { return default(bool); } }
        public virtual bool CheckCertRevocationStatus { get { return default(bool); } }
        public virtual System.Security.Authentication.CipherAlgorithmType CipherAlgorithm { get { return default(System.Security.Authentication.CipherAlgorithmType); } }
        public virtual int CipherStrength { get { return default(int); } }
        public virtual System.Security.Authentication.HashAlgorithmType HashAlgorithm { get { return default(System.Security.Authentication.HashAlgorithmType); } }
        public virtual int HashStrength { get { return default(int); } }
        public override bool IsAuthenticated { get { return default(bool); } }
        public override bool IsEncrypted { get { return default(bool); } }
        public override bool IsMutuallyAuthenticated { get { return default(bool); } }
        public override bool IsServer { get { return default(bool); } }
        public override bool IsSigned { get { return default(bool); } }
        public virtual System.Security.Authentication.ExchangeAlgorithmType KeyExchangeAlgorithm { get { return default(System.Security.Authentication.ExchangeAlgorithmType); } }
        public virtual int KeyExchangeStrength { get { return default(int); } }
        public override long Length { get { return default(long); } }
        public virtual System.Security.Cryptography.X509Certificates.X509Certificate LocalCertificate { get { return default(System.Security.Cryptography.X509Certificates.X509Certificate); } }
        public override long Position { get { return default(long); } set { } }
        public override int ReadTimeout { get { return default(int); } set { } }
        public virtual System.Security.Cryptography.X509Certificates.X509Certificate RemoteCertificate { get { return default(System.Security.Cryptography.X509Certificates.X509Certificate); } }
        public virtual System.Security.Authentication.SslProtocols SslProtocol { get { return default(System.Security.Authentication.SslProtocols); } }
        public System.Net.TransportContext TransportContext { get { return default(System.Net.TransportContext); } }
        public override int WriteTimeout { get { return default(int); } set { } }
        public virtual System.Threading.Tasks.Task AuthenticateAsClientAsync(string targetHost) { return default(System.Threading.Tasks.Task); }
        public virtual System.Threading.Tasks.Task AuthenticateAsClientAsync(string targetHost, System.Security.Cryptography.X509Certificates.X509CertificateCollection clientCertificates, System.Security.Authentication.SslProtocols enabledSslProtocols, bool checkCertificateRevocation) { return default(System.Threading.Tasks.Task); }
        public virtual System.Threading.Tasks.Task AuthenticateAsServerAsync(System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate) { return default(System.Threading.Tasks.Task); }
        public virtual System.Threading.Tasks.Task AuthenticateAsServerAsync(System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate, bool clientCertificateRequired, System.Security.Authentication.SslProtocols enabledSslProtocols, bool checkCertificateRevocation) { return default(System.Threading.Tasks.Task); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) { return default(int); }
        public override long Seek(long offset, System.IO.SeekOrigin origin) { return default(long); }
        public override void SetLength(long value) { }
        public void Write(byte[] buffer) { }
        public override void Write(byte[] buffer, int offset, int count) { }
    }
    
    public partial class AuthenticationException : System.Exception
    {
        public AuthenticationException() { }
        public AuthenticationException(string message) { }
        public AuthenticationException(string message, System.Exception innerException) { }
    }
    
    public enum EncryptionPolicy
    {
        AllowNoEncryption = 1,
        NoEncryption = 2,
        RequireEncryption = 0,
    }
}