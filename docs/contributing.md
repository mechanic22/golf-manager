# Contributing Guide

## Development Setup

### Prerequisites
- .NET 10 SDK
- Node.js 18+ and npm
- Visual Studio 2022 or VS Code
- Git

### Quick Start
```bash
# Clone repository
git clone <repository-url>
cd golf-manager

# Install dependencies
npm install
dotnet restore

# Setup database
cd src/GolfManager.Api
dotnet ef database update

# Run development servers
# Terminal 1: API
dotnet watch run

# Terminal 2: Web client
cd ../GolfManager.Web
dotnet watch run

# Terminal 3: CSS watch
npm run watch:css
```

## Development Workflow

### Branch Strategy
- `main` - Production-ready code
- `develop` - Integration branch
- `feature/*` - Feature branches
- `bugfix/*` - Bug fix branches
- `release/*` - Release preparation

### Commit Guidelines
```bash
# Format: type(scope): description
feat(auth): add JWT refresh token support
fix(scoring): resolve handicap calculation bug
docs(api): update endpoint documentation
```

### Pull Request Process
1. Create feature branch from `develop`
2. Implement changes with tests
3. Ensure CI passes
4. Create PR with description
5. Code review and approval
6. Merge to `develop`

## Code Standards

### C# Guidelines
- Use C# 12 features and patterns
- Follow .NET naming conventions
- Use async/await for I/O operations
- Implement proper error handling
- Use dependency injection

### API Design
- RESTful endpoint naming
- Consistent response format with `ApiResponse<T>`
- Proper HTTP status codes
- Input validation with data annotations
- Swagger documentation for all endpoints

### Frontend Guidelines
- Use Blazor component patterns
- Material Design 3 components from FifthBox library
- Tailwind CSS for styling
- Client-side validation
- Responsive design principles

## Testing

### Unit Tests
```bash
# Run API tests
cd tests/GolfManager.UnitTests
dotnet test

# Run integration tests
cd GolfManager.IntegrationTests
dotnet test
```

### Test Coverage
- Aim for 80%+ code coverage
- Test business logic thoroughly
- Mock external dependencies
- Include edge cases and error scenarios

## Database Development

### Migrations
```bash
# Create migration
cd src/GolfManager.Api
dotnet ef migrations add MigrationName

# Apply migration
dotnet ef database update

# Revert migration
dotnet ef database update PreviousMigration
```

### Data Seeding
- Use `DataSeeder` for development data
- Separate seed data from migrations
- Include realistic test data

## Code Review Checklist

### API Changes
- [ ] Proper authorization attributes
- [ ] Input validation
- [ ] Error handling
- [ ] Swagger documentation
- [ ] Unit tests added

### Frontend Changes
- [ ] Responsive design
- [ ] Accessibility compliance
- [ ] Material Design guidelines
- [ ] Performance considerations

### Database Changes
- [ ] Migration scripts tested
- [ ] Backward compatibility
- [ ] Data integrity preserved
- [ ] Rollback plan documented

## Performance Guidelines

### API Performance
- Use `async/await` for scalability
- Implement caching where appropriate
- Optimize database queries
- Use pagination for large datasets
- Monitor response times

### Frontend Performance
- Lazy load components
- Optimize bundle size
- Use virtualization for lists
- Minimize re-renders
- Cache static assets

## Security Considerations

### Authentication
- JWT tokens with proper expiration
- Secure password policies
- Multi-factor authentication support
- Session management

### Authorization
- Role-based access control
- Resource-level permissions
- Input sanitization
- CORS configuration

### Data Protection
- Encrypt sensitive data
- Secure connection strings
- Audit logging
- GDPR compliance

## Documentation

### Code Documentation
- XML comments for public APIs
- README updates for features
- Inline comments for complex logic
- API documentation with Swagger

### External Documentation
- Update docs/ for architectural changes
- Include examples and usage patterns
- Keep deployment guides current

## Issue Tracking

### Bug Reports
- Clear reproduction steps
- Environment details
- Expected vs actual behavior
- Screenshots/logs when relevant

### Feature Requests
- Clear problem statement
- Proposed solution
- Alternative approaches considered
- Impact assessment

## Release Process

### Version Numbering
- Semantic versioning (MAJOR.MINOR.PATCH)
- Major: Breaking changes
- Minor: New features
- Patch: Bug fixes

### Release Checklist
- [ ] All tests passing
- [ ] Documentation updated
- [ ] Migration scripts tested
- [ ] Performance benchmarks
- [ ] Security review completed
- [ ] Deployment verified

## Getting Help

### Communication
- GitHub Issues for bugs/features
- Discord/Slack for questions
- Email for security issues

### Resources
- [API Documentation](./api.md)
- [Architecture Overview](./architecture.md)
- [Deployment Guide](./deployment.md)
- [Quick Start](../QUICK_START.md)</content>
<parameter name="filePath">/Users/tonygilbert/Projects/code/5thbox/dkgolf/golf-manager/docs/contributing.md