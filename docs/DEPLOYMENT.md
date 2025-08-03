# Superelf CI/CD Deployment Setup

This document provides an overview of the automated CI/CD setup for the Superelf application using GitHub Actions, Docker, and Nginx.

## Architecture Overview

The application is deployed to a single Ubuntu 20.04 server running 6 Docker containers:

### Production Environment (`superelf.yorickteriele.nl`)
- **superelf-postgres-prod**: PostgreSQL database (port 5432)
- **superelf-api-prod**: .NET 8 API (port 5000)
- **superelf-frontend-prod**: React frontend (port 3000)

### Test Environment (`superelf-test.yorickteriele.nl`)
- **superelf-postgres-test**: PostgreSQL database (port 5433)
- **superelf-api-test**: .NET 8 API (port 5001)
- **superelf-frontend-test**: React frontend (port 3001)

All traffic is routed through Nginx with SSL/TLS termination using Let's Encrypt certificates.

## Deployment Flow

### Production Deployment
- **Trigger**: Push to `master` branch
- **Domain**: `superelf.yorickteriele.nl`
- **Workflow**: `.github/workflows/deploy-prod.yml`

### Test Deployment
- **Trigger**: Push to `develop` branch
- **Domain**: `superelf-test.yorickteriele.nl`
- **Workflow**: `.github/workflows/deploy-test.yml`

## Required GitHub Secrets

The following secrets must be configured in your GitHub repository:

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `SSH_PRIVATE_KEY` | Private SSH key for server access | `-----BEGIN OPENSSH PRIVATE KEY-----...` |
| `SERVER_HOST` | Server IP address or hostname | `123.456.789.0` or `server.example.com` |
| `PROD_DB_PASSWORD` | Production database password | `super_secure_prod_password_123` |
| `PROD_JWT_SECRET` | Production JWT signing key (min 32 chars) | `very_long_and_secure_jwt_secret_for_production` |
| `TEST_DB_PASSWORD` | Test database password | `super_secure_test_password_123` |
| `TEST_JWT_SECRET` | Test JWT signing key (min 32 chars) | `very_long_and_secure_jwt_secret_for_testing` |

## File Structure

```
├── .github/workflows/
│   ├── deploy-prod.yml          # Production deployment workflow
│   └── deploy-test.yml          # Test deployment workflow
├── docs/
│   └── SERVER_SETUP.md          # Server setup guide
├── nginx/
│   └── superelf.conf            # Nginx reverse proxy configuration
├── scripts/
│   └── deploy-local.sh          # Local deployment testing script
├── Superelf.API/
│   └── Dockerfile               # .NET API Dockerfile
├── Superelf.Web/
│   ├── Dockerfile               # React frontend Dockerfile
│   └── nginx.conf               # Frontend nginx configuration
├── docker-compose.prod.yml      # Production containers
├── docker-compose.test.yml      # Test containers
├── .dockerignore                # Docker ignore patterns
├── .env.prod.example            # Production environment template
└── .env.test.example            # Test environment template
```

## Deployment Process

1. **Code Push**: Developer pushes code to `master` (prod) or `develop` (test)
2. **GitHub Actions**: Workflow is triggered automatically
3. **Build**: Code is packaged and sent to the server
4. **Stop**: Old containers are stopped and removed
5. **Deploy**: New containers are built and started
6. **Health Check**: API health is verified
7. **Cleanup**: Temporary files are removed

## Local Development

### Prerequisites
- Docker and Docker Compose installed
- Environment files created from templates

### Testing Deployments Locally

1. Create environment files:
   ```bash
   cp .env.prod.example .env.prod
   cp .env.test.example .env.test
   # Edit files with actual values
   ```

2. Run local deployment test:
   ```bash
   # Test production setup
   chmod +x scripts/deploy-local.sh
   ./scripts/deploy-local.sh prod
   
   # Test test setup
   ./scripts/deploy-local.sh test
   ```

### Manual Container Management

```bash
# Start production environment
docker-compose -f docker-compose.prod.yml up -d --build

# View logs
docker-compose -f docker-compose.prod.yml logs -f

# Stop environment
docker-compose -f docker-compose.prod.yml down

# Clean up
docker system prune -a -f
```

## Server Setup

Follow the detailed server setup guide in [`docs/SERVER_SETUP.md`](docs/SERVER_SETUP.md) to prepare your Ubuntu server for automated deployments.

## Security Features

- **HTTPS Only**: All traffic redirected to HTTPS with HSTS headers
- **Rate Limiting**: API and general request limiting via Nginx
- **Security Headers**: XSS protection, content type sniffing prevention
- **Firewall**: UFW configured to allow only necessary ports
- **Non-root Containers**: All containers run as non-root users
- **Secrets Management**: Sensitive data stored in GitHub Secrets
- **SSL Auto-renewal**: Let's Encrypt certificates auto-renew

## Monitoring and Maintenance

### Health Checks
- **Database**: PostgreSQL health checks with retry logic
- **API**: HTTP health endpoint monitoring
- **SSL**: Automatic certificate renewal monitoring

### Log Management
- **Docker Logs**: Configured with rotation (10MB, 3 files)
- **Nginx Logs**: Standard access and error logging
- **Application Logs**: Available via `docker-compose logs`

### Backup Recommendations
- Database volumes should be backed up regularly
- Consider automated backup scripts for PostgreSQL data
- Monitor disk space usage for log files

## Troubleshooting

### Common Issues

1. **Deployment Fails**: Check GitHub Actions logs for errors
2. **Container Won't Start**: Check Docker logs: `docker-compose logs`
3. **Database Connection**: Verify environment variables and network connectivity
4. **SSL Issues**: Ensure DNS points to server before requesting certificates
5. **Port Conflicts**: Verify ports 3000, 3001, 5000, 5001, 5432, 5433 are available

### Debugging Commands

```bash
# Check container status
docker ps -a

# View container logs
docker logs <container_name>

# Check nginx configuration
sudo nginx -t

# Test SSL certificates
curl -I https://superelf.yorickteriele.nl

# Check firewall status
sudo ufw status verbose

# Monitor resources
htop
docker stats
```

## Contributing

When contributing to this project:

1. **Development**: Work on feature branches
2. **Testing**: Push to `develop` branch for test environment deployment
3. **Production**: Merge to `master` branch for production deployment
4. **Environment Variables**: Never commit actual secrets to the repository
5. **Testing**: Test locally using the provided scripts before pushing

## Support

For issues with the deployment setup:

1. Check the server setup guide
2. Review GitHub Actions workflow logs
3. Verify all secrets are properly configured
4. Test local deployment first
5. Check server logs and container status

## License

This deployment configuration is part of the Superelf project and follows the same license terms.
