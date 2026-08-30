# Use a lightweight Eclipse Temurin Alpine image optimized for Java 21
FROM eclipse-temurin:21-jre-alpine

# Create a non-root system user for security purposes
RUN addgroup -S spring && adduser -S spring -G spring
USER spring:spring

# Set the working directory inside the container
WORKDIR /app

# Copy the generated Maven JAR file into the container
# Note: Replace 'my-soap-service' with the actual artifactId from your pom.xml
COPY target/transaction-api-0.0.1-SNAPSHOT.jar app.jar

# SOAP services usually run on port 8080 by default
EXPOSE 8080

# Execute the application with optimized memory settings
ENTRYPOINT ["java", "-jar", "app.jar"]