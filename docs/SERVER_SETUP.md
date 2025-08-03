# Superelf Server Setup Guide

This guide covers the manual setup steps required on your Ubuntu 20.04 server before the automated CI/CD deployments can work.

## Prerequisites

- Ubuntu 20.04 server with root access
- Domain names pointing to your server:
  - `superelf.yorickteriele.nl` → Production
  - `superelf-test.yorickteriele.nl` → Test
- SSH key pair for GitHub Actions (already configured in GitHub Secrets)

## 1. Initial Server Setup

### Update system packages
```bash
apt update && apt upgrade -y
```

### Setup SSH access for GitHub Actions
```bash
# Create SSH directory (if it doesn't exist)
mkdir -p ~/.ssh
chmod 700 ~/.ssh

# Add your GitHub Actions public key to authorized_keys
# (The public key that corresponds to SSH_PRIVATE_KEY secret)
# You can edit this file with nano or echo the public key:
nano ~/.ssh/authorized_keys
chmod 600 ~/.ssh/authorized_keys

# Ensure SSH service is running and enabled
systemctl start ssh
systemctl enable ssh
```

## 2. Install Docker and Docker Compose

### Install Docker
```bash
# Remove old versions
apt remove docker docker-engine docker.io containerd runc

# Install dependencies
apt update
apt install -y apt-transport-https ca-certificates curl gnupg lsb-release

# Add Docker's official GPG key
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg

# Add Docker repository
echo "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null

# Install Docker
apt update
apt install -y docker-ce docker-ce-cli containerd.io

# Start and enable Docker
systemctl start docker
systemctl enable docker

# Verify Docker installation
docker --version
```

### Install Docker Compose
```bash
# Download Docker Compose (latest stable version)
curl -L "https://github.com/docker/compose/releases/download/v2.24.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose

# Make it executable
chmod +x /usr/local/bin/docker-compose

# Create symlink for easier access
ln -sf /usr/local/bin/docker-compose /usr/bin/docker-compose

# Verify installation
docker-compose --version
```

## 3. Install and Configure Nginx

### Install Nginx
```bash
apt install -y nginx
systemctl start nginx
systemctl enable nginx
```

### Create basic Nginx configuration
```bash
# Remove default site
rm -f /etc/nginx/sites-enabled/default

# Create our configuration directory structure
mkdir -p /etc/nginx/sites-available
mkdir -p /etc/nginx/sites-enabled

# Create temporary configuration (we'll update this later with the proper config)
cat > /etc/nginx/sites-available/superelf << 'EOF'
server {
    listen 80;
    server_name superelf.yorickteriele.nl;
    
    location / {
        return 301 https://$server_name$request_uri;
    }
    
    location ~/.well-known/acme-challenge {
        root /var/www/html;
    }
}

server {
    listen 80;
    server_name superelf-test.yorickteriele.nl;
    
    location / {
        return 301 https://$server_name$request_uri;
    }
    
    location ~/.well-known/acme-challenge {
        root /var/www/html;
    }
}
EOF

# Enable the site
ln -s /etc/nginx/sites-available/superelf /etc/nginx/sites-enabled/

# Test configuration
nginx -t

# Restart nginx
systemctl restart nginx
```

## 4. Setup Let's Encrypt SSL Certificates

### Install Certbot
```bash
apt install -y snapd
snap install core
snap refresh core
snap install --classic certbot
ln -s /snap/bin/certbot /usr/bin/certbot
```

### Obtain SSL certificates
```bash
# Create webroot directory for verification
mkdir -p /var/www/html

# Get certificates for both domains (using webroot method)
certbot certonly --webroot -w /var/www/html -d superelf.yorickteriele.nl
certbot certonly --webroot -w /var/www/html -d superelf-test.yorickteriele.nl

# If webroot method fails, try standalone method:
# systemctl stop nginx
# certbot certonly --standalone -d superelf.yorickteriele.nl
# certbot certonly --standalone -d superelf-test.yorickteriele.nl
# systemctl start nginx
```

### Update Nginx configuration with SSL
```bash
# Now create the full configuration with SSL
# This overwrites the temporary config we created earlier
cat > /etc/nginx/sites-available/superelf << 'EOF'
# Production - superelf.yorickteriele.nl
server {
    listen 80;
    server_name superelf.yorickteriele.nl;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name superelf.yorickteriele.nl;
    
    ssl_certificate /etc/letsencrypt/live/superelf.yorickteriele.nl/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/superelf.yorickteriele.nl/privkey.pem;
    
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;
    
    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
    
    location /api/ {
        proxy_pass http://localhost:5000/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
    
    location /hub/ {
        proxy_pass http://localhost:5000/hub/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}

# Test - superelf-test.yorickteriele.nl
server {
    listen 80;
    server_name superelf-test.yorickteriele.nl;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name superelf-test.yorickteriele.nl;
    
    ssl_certificate /etc/letsencrypt/live/superelf-test.yorickteriele.nl/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/superelf-test.yorickteriele.nl/privkey.pem;
    
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;
    
    location / {
        proxy_pass http://localhost:3001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
    
    location /api/ {
        proxy_pass http://localhost:5001/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
    
    location /hub/ {
        proxy_pass http://localhost:5001/hub/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
EOF

# Test the new configuration
nginx -t

# If the test passes, reload nginx with the new configuration
systemctl reload nginx

# Note: If you need to edit this configuration later, use:
# nano /etc/nginx/sites-available/superelf
# Then test with: nginx -t
# Then reload with: systemctl reload nginx
```

### Setup auto-renewal
```bash
# Test renewal
certbot renew --dry-run

# Add to crontab for automatic renewal
crontab -e

# Add this line to renew certificates twice daily:
0 12 * * * /usr/bin/certbot renew --quiet --renew-hook "systemctl reload nginx"
```

## Important Nginx Configuration Files

After completing the SSL setup, you'll have these key Nginx files:

- **Main configuration**: `/etc/nginx/sites-available/superelf`
  - This is the file you edit when you need to make changes
  - Edit with: `nano /etc/nginx/sites-available/superelf`

- **Active configuration**: `/etc/nginx/sites-enabled/superelf`
  - This is a symbolic link to the file above
  - Don't edit this directly

- **Test changes**: Always test after editing: `nginx -t`
- **Apply changes**: After testing: `systemctl reload nginx`

## 5. Configure Firewall

### Setup UFW (Uncomplicated Firewall)
```bash
# Install UFW if not already installed
apt install -y ufw

# Reset UFW to defaults
ufw --force reset

# Set default policies
ufw default deny incoming
ufw default allow outgoing

# Allow SSH (adjust port if you changed it)
ufw allow 22/tcp

# Allow HTTP and HTTPS
ufw allow 80/tcp
ufw allow 443/tcp

# Allow Docker subnet communication (for internal container communication)
ufw allow from 172.16.0.0/12

# Enable UFW
ufw --force enable

# Check status
ufw status verbose
```

## 6. Create Deployment Directories

```bash
# Create deployment directories in root home
mkdir -p /root/superelf-prod
mkdir -p /root/superelf-test

# Set proper permissions
chmod 755 /root/superelf-prod
chmod 755 /root/superelf-test

# Create directories for persistent data
mkdir -p /var/lib/superelf-prod/db
mkdir -p /var/lib/superelf-test/db
chmod 755 /var/lib/superelf-prod/db
chmod 755 /var/lib/superelf-test/db
```

## 7. Configure Docker Logging

```bash
# Create Docker daemon configuration for log rotation
mkdir -p /etc/docker
cat > /etc/docker/daemon.json << 'EOF'
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  }
}
EOF

# Restart Docker
systemctl restart docker

# Verify Docker is running
systemctl status docker
```

## 8. Setup System Monitoring (Optional but Recommended)

### Install basic monitoring tools
```bash
apt install -y htop iotop nethogs
```

### Create log monitoring script
```bash
# Create a simple script to check application logs
cat > /usr/local/bin/superelf-logs << 'EOF'
#!/bin/bash
echo "=== Production Logs ==="
cd /root/superelf-prod
docker-compose -f docker-compose.prod.yml logs --tail=50

echo "=== Test Logs ==="
cd /root/superelf-test
docker-compose -f docker-compose.test.yml logs --tail=50
EOF

chmod +x /usr/local/bin/superelf-logs
```

## 9. GitHub Secrets Configuration

Make sure these secrets are configured in your GitHub repository settings:

### Server Access:
- `SSH_PRIVATE_KEY`: Your SSH private key (corresponding public key should be in `/root/.ssh/authorized_keys`)
- `SERVER_HOST`: Your server IP address or hostname (same for both prod and test since they're on the same server)

### Production Environment:
- `PROD_DB_PASSWORD`: Strong password for production database
- `PROD_JWT_SECRET`: Long random string for JWT signing

### Test Environment:
- `TEST_DB_PASSWORD`: Strong password for test database  
- `TEST_JWT_SECRET`: Long random string for JWT signing (different from prod)

### Optional (if using separate servers):
- `PRODUCTION_SERVER_HOST`: Production server IP/hostname
- `TEST_SERVER_HOST`: Test server IP/hostname

## 10. Test the Setup

### Test Docker
```bash
docker run hello-world
```

### Test Docker Compose
```bash
docker-compose --version
```

### Test Nginx
```bash
nginx -t
curl -I http://localhost
```

### Test SSL (after certificates are installed)
```bash  
curl -I https://superelf.yorickteriele.nl
curl -I https://superelf-test.yorickteriele.nl
```

### Test firewall
```bash
ufw status verbose
```

## 11. Initial Manual Deployment Test (Optional)

Before running the GitHub Actions, you can test a manual deployment:

```bash
# Clone your repository for testing
cd /root
git clone https://github.com/yorickteriele/Superelf.git superelf-manual-test
cd superelf-manual-test

# Create environment file for testing
cat > .env << EOF
PROD_DB_PASSWORD=your_test_password
PROD_JWT_SECRET=your_test_jwt_secret
EOF

# Test build (make sure docker-compose files exist)
ls -la docker-compose*.yml

# If docker-compose files exist, test build
docker-compose -f docker-compose.prod.yml config

# Clean up test
cd /root
rm -rf superelf-manual-test
```

## 12. Maintenance Commands

### View application logs
```bash
# Production
cd /root/superelf-prod
docker-compose -f docker-compose.prod.yml logs -f

# Test
cd /root/superelf-test
docker-compose -f docker-compose.test.yml logs -f

# Or use the monitoring script
superelf-logs
```

### Check container status
```bash
# Production
cd /root/superelf-prod
docker-compose -f docker-compose.prod.yml ps

# Test
cd /root/superelf-test
docker-compose -f docker-compose.test.yml ps
```

### Restart services
```bash
# Production
cd /root/superelf-prod
docker-compose -f docker-compose.prod.yml restart

# Test
cd /root/superelf-test
docker-compose -f docker-compose.test.yml restart
```

### Update containers manually
```bash
# Production
cd /root/superelf-prod
docker-compose -f docker-compose.prod.yml down
docker-compose -f docker-compose.prod.yml up -d --build

# Test
cd /root/superelf-test
docker-compose -f docker-compose.test.yml down
docker-compose -f docker-compose.test.yml up -d --build
```

### Clean up Docker resources
```bash
# Remove unused containers, networks, and images
docker system prune -a -f

# Remove unused volumes (be careful with database volumes!)
docker volume prune -f

# Check disk usage
docker system df
```

### Check system resources
```bash
# Check disk space
df -h

# Check memory usage
free -h

# Check running processes
htop

# Check network connections
netstat -tlnp
```

### Edit Nginx configuration
```bash
# Edit the main configuration file
nano /etc/nginx/sites-available/superelf

# After making changes, always test the configuration
nginx -t

# If test passes, reload nginx
systemctl reload nginx

# View nginx status
systemctl status nginx

# Check nginx error logs if needed
tail -f /var/log/nginx/error.log
```

## Security Notes

1. **Database passwords**: Make sure to use strong, unique passwords for `PROD_DB_PASSWORD` and `TEST_DB_PASSWORD`
2. **JWT secrets**: Use long, random strings for `PROD_JWT_SECRET` and `TEST_JWT_SECRET`
3. **SSH keys**: Keep your SSH private key secure and consider using SSH key rotation
4. **Firewall**: Only necessary ports (22, 80, 443) are opened
5. **Updates**: Regularly update the system and Docker:
   ```bash
   apt update && apt upgrade -y
   ```
6. **Backups**: Set up regular database backups for the SQL Server volumes:
   ```bash
   # Example backup script
   docker exec superelf-db-prod /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$PROD_DB_PASSWORD" -Q "BACKUP DATABASE [Superelf] TO DISK = '/var/opt/mssql/backup/superelf-prod-$(date +%Y%m%d_%H%M%S).bak'"
   ```
7. **Root access**: Since we're using root, ensure SSH is properly secured and consider using SSH key authentication only

## Troubleshooting

### Common issues:

1. **Permission denied**: 
   - Make sure you're running as root or using appropriate permissions
   - Check file permissions: `ls -la /root/.ssh/`
   - Verify SSH key is properly formatted (no extra spaces/characters)

2. **Port conflicts**: 
   - Check what's using ports: `netstat -tlnp | grep ":3000\|:3001\|:5000\|:5001"`
   - Ensure ports 3000, 3001, 5000, 5001, 1433, 1434 are available
   - Kill processes if needed: `fuser -k 3000/tcp`

3. **SSL issues**: 
   - Verify DNS is pointing to your server: `nslookup superelf.yorickteriele.nl`
   - Check certificate files exist: `ls -la /etc/letsencrypt/live/`
   - Test certificate: `openssl x509 -in /etc/letsencrypt/live/superelf.yorickteriele.nl/cert.pem -text -noout`

4. **Docker issues**: 
   - Check Docker daemon status: `systemctl status docker`
   - Check Docker logs: `journalctl -u docker --no-pager`
   - Restart Docker: `systemctl restart docker`
   - Check Docker disk usage: `docker system df`

5. **Nginx issues**: 
   - Test configuration: `nginx -t`
   - Check Nginx logs: `journalctl -u nginx --no-pager`
   - Check error log: `tail -f /var/log/nginx/error.log`
   - Restart Nginx: `systemctl restart nginx`

6. **Firewall blocking connections**:
   - Check UFW status: `ufw status verbose`
   - Check iptables: `iptables -L`
   - Temporarily disable UFW for testing: `ufw disable` (remember to re-enable!)

7. **Database connection issues**:
   - Check if SQL Server containers are running: `docker ps | grep sql`
   - Check container logs: `docker logs <container_name>`
   - Verify database passwords in environment variables

8. **GitHub Actions deployment fails**:
   - Check SSH connection: `ssh -T git@github.com`
   - Verify GitHub secrets are set correctly
   - Check server logs during deployment
   - Ensure deployment directories exist with proper permissions

### Useful debugging commands:

```bash
# Check all running processes
ps aux

# Check disk usage
df -h
du -sh /root/superelf-*
du -sh /var/lib/docker

# Check memory usage
free -h

# Check network connections
ss -tlnp

# Check system logs
journalctl --since "1 hour ago"

# Check Docker container status
docker ps -a
docker stats

# Monitor real-time logs
tail -f /var/log/nginx/access.log
tail -f /var/log/nginx/error.log
```

This completes the updated server setup guide. After completing these steps, your GitHub Actions workflows should be able to automatically deploy your application using the root user.
