# Security Policy

## Supported Versions

We actively support the following versions of Schulkueche Monatsabrechnung with security updates:

| Version | Supported          | Support Status |
| ------- | ------------------ | -------------- |
| 1.3.x   | :white_check_mark: | Current stable |
| 1.2.x   | :white_check_mark: | LTS support    |
| 1.1.x   | :x:                | End of life    |
| < 1.1   | :x:                | End of life    |

### Version Support Policy

- **Current Stable (1.3.x)**: Full security support with immediate patches
- **LTS Support (1.2.x)**: Critical security fixes only until 2026-Q2
- **End of Life**: No security updates provided

## Reporting a Vulnerability

We take the security of Schulkueche Monatsabrechnung seriously. If you discover a security vulnerability, we appreciate your help in disclosing it responsibly.

### How to Report

#### For Critical/High Severity Issues

**DO NOT** create a public GitHub issue for security vulnerabilities.

Instead, please report security vulnerabilities through one of these methods:

1. **GitHub Security Advisories** (Preferred)
   - Go to the [Security tab](https://github.com/Zobiii/SchulkuecheMonatsabrechnung/security) in our repository
   - Click "Report a vulnerability"
   - Fill out the private security advisory form

2. **Email** (Alternative)
   - Create a GitHub issue titled "Security: Request for Private Contact"
   - Include minimal details (just that it's a security issue)
   - A maintainer will provide a secure contact method
   - **Do not include vulnerability details in the initial issue**

#### For Low Severity Issues

For low-impact security issues or security-related feature requests:
- Create a regular GitHub issue with the "security" label
- Use the title format: `Security: [Brief Description]`

### What to Include

When reporting a vulnerability, please provide:

- **Vulnerability Type**: What kind of security issue is this?
- **Impact Assessment**: What could an attacker accomplish?
- **Affected Components**: Which parts of the application are affected?
- **Reproduction Steps**: Detailed steps to reproduce the issue
- **Proof of Concept**: If applicable (but avoid harmful exploits)
- **Suggested Fix**: If you have ideas for remediation
- **Environment Details**: OS, .NET version, application version

### Response Timeline

We strive to respond to security reports according to the following timeline:

| Severity | Initial Response | Status Update | Fix Timeline |
|----------|-----------------|---------------|--------------|
| Critical | 24 hours        | Weekly        | 7 days       |
| High     | 72 hours        | Bi-weekly     | 30 days      |
| Medium   | 1 week          | Monthly       | 90 days      |
| Low      | 2 weeks         | As needed     | Next release |

### Severity Guidelines

#### Critical
- Remote code execution
- SQL injection leading to data breach
- Authentication bypass
- Data loss or corruption

#### High  
- Local privilege escalation
- Cross-site scripting (XSS) with account takeover
- Information disclosure of sensitive data
- Denial of service affecting all users

#### Medium
- Information disclosure of non-sensitive data
- Denial of service affecting some users
- CSRF with limited impact
- Security misconfiguration

#### Low
- Information disclosure with minimal impact
- Security best practice violations
- Deprecated dependency usage

## Security Measures

### Application Security

#### Authentication & Authorization
- **User Authentication**: Secure password hashing using industry standards
- **Session Management**: Secure session handling with proper timeout
- **Input Validation**: All user inputs are validated and sanitized
- **SQL Injection Protection**: Parameterized queries and Entity Framework protection

#### Data Protection
- **Database Security**: SQLite database with appropriate file permissions
- **Data Encryption**: Sensitive data encrypted at rest
- **Backup Security**: Secure handling of database backups
- **PII Protection**: Personal information handling according to privacy standards

#### Infrastructure Security
- **Dependency Management**: Regular updates of NuGet packages
- **Code Analysis**: Static code analysis for security vulnerabilities
- **Build Security**: Secure build pipeline and artifact signing
- **Supply Chain**: Verification of dependencies and build tools

### Development Security

#### Secure Development Lifecycle
- **Security Reviews**: Code review process includes security considerations
- **Dependency Scanning**: Automated scanning for vulnerable dependencies
- **Static Analysis**: Integration of security-focused static analysis tools
- **Security Testing**: Regular security testing of critical functionality

#### Third-Party Dependencies
- **Avalonia UI**: UI framework security depends on Avalonia project
- **Entity Framework**: Database access security through Microsoft EF Core
- **QuestPDF**: PDF generation security through QuestPDF library
- **CommunityToolkit.Mvvm**: MVVM framework security

Regular security audits of dependencies are performed during each release cycle.

## Security Best Practices for Users

### Installation Security
- **Download Sources**: Only download from official GitHub releases
- **Checksum Verification**: Verify release checksums when provided
- **Administrator Rights**: Run installation with appropriate privileges only
- **Antivirus Scanning**: Scan downloaded files with updated antivirus

### Usage Security
- **Regular Updates**: Keep the application updated to latest supported version
- **Database Backups**: Regularly backup your kitchen.db database
- **Access Control**: Limit access to the application data directory
- **Network Security**: Be cautious when sharing PDF exports containing personal data

### Data Security
- **Personal Information**: The application handles personal data (names, addresses)
- **GDPR Compliance**: Users are responsible for GDPR compliance in their jurisdiction
- **Data Retention**: Implement appropriate data retention policies
- **Export Security**: Secure handling of PDF exports containing sensitive data

## Known Security Considerations

### Current Limitations
- **Single-User Design**: Application designed for single-user/single-machine usage
- **Local Storage**: Data stored locally in SQLite database
- **No Network Encryption**: Application doesn't include network communication
- **PDF Security**: Generated PDFs contain unencrypted personal information

### Future Security Enhancements
- **Multi-User Authentication**: Planned for v2.0.x
- **Data Encryption**: Enhanced encryption for sensitive fields
- **Audit Logging**: Security event logging and monitoring
- **Access Controls**: Role-based access control system

## Security-Related Configuration

### Database Security
```bash
# Default database location (Windows)
%LocalAppData%/Schulkueche/kitchen.db

# Recommended file permissions (restrict access to current user only)
icacls "%LocalAppData%\Schulkueche\kitchen.db" /inheritance:r /grant:r "%USERNAME%:(R,W)"
```

### PDF Export Security
```bash
# Default export location
~/Documents/Schulkueche/

# Consider encrypting exported PDFs for sensitive data
# Use third-party tools for PDF encryption if required
```

## Incident Response

### In Case of Security Breach

If you suspect a security incident:

1. **Immediate Actions**
   - Stop using the affected functionality
   - Secure your database and exported files
   - Document the suspected incident

2. **Reporting**
   - Report the incident using our vulnerability reporting process
   - Provide as much detail as possible about the incident
   - Include steps taken to mitigate the issue

3. **Recovery**
   - Follow our incident response guidance (provided upon report)
   - Update to the latest secure version when available
   - Review and update your security practices

## Security Updates

### Update Notifications
- **GitHub Releases**: Security updates announced in release notes
- **GitHub Security Advisories**: Critical vulnerabilities published as advisories
- **Issue Tracking**: Security-related issues labeled for visibility

### Update Process
1. **Backup**: Always backup your database before updating
2. **Download**: Download updates from official GitHub releases only
3. **Verify**: Check release notes for security-specific changes
4. **Test**: Test functionality in a non-production environment first
5. **Deploy**: Apply updates to production systems

## Contact Information

### Security Team
- **Primary Contact**: Repository maintainer (Lechner Tobias)
- **Response Method**: GitHub Security Advisories or Issues
- **Public Key**: Not currently available (contact for secure communication needs)

### Emergency Contact
For critical security issues requiring immediate attention:
- Create a GitHub issue with title "URGENT SECURITY ISSUE - REQUEST IMMEDIATE CONTACT"
- Include minimal details and request private communication channel
- Monitor the issue for response within 24 hours

## Acknowledgments

We appreciate the security research community and will acknowledge responsible disclosure of vulnerabilities:

- **Public Thanks**: Listed in release notes (with permission)
- **Security Hall of Fame**: Maintained in repository documentation (when established)
- **CVE Credits**: Appropriate credit in CVE assignments when applicable

### Hall of Fame
*This section will list security researchers who have responsibly disclosed vulnerabilities.*

Currently no entries - be the first to help improve our security!

## Legal

### Safe Harbor
We support safe harbor for security researchers who:

- Make a good faith effort to avoid privacy violations and data destruction
- Report vulnerabilities responsibly according to this policy
- Do not access unnecessary amounts of data
- Do not modify or delete data
- Do not disrupt our services or other users

### Scope
This security policy applies to:

- **SchulkuecheApp application code** in this repository
- **Official releases** distributed through GitHub
- **Documentation and related materials**

This policy does not cover:
- Third-party dependencies (report to respective maintainers)
- User-modified versions of the software
- Deployment environments outside our control

---

Thank you for helping keep Schulkueche Monatsabrechnung and our users secure!