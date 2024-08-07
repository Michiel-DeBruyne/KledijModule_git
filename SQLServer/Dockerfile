# Use the official SQL Server 2022 image as a base
FROM mcr.microsoft.com/mssql/server:2022-latest

# Set environment variables
ENV SA_PASSWORD=YourStrong@Passw0rd
ENV ACCEPT_EULA=Y
ENV MSSQL_PID=Developer

# Create the log directory
RUN mkdir -p /var/opt/mssql/log

# Expose SQL Server port
EXPOSE 1433

# Default command to start SQL Server
CMD ["/opt/mssql/bin/sqlservr"]
