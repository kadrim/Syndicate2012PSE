FROM mcr.microsoft.com/dotnet/sdk:8.0

RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    libice6 \
    libsm6 \
    libx11-6 \
    libxext6 \
    libxrandr2 \
    libxi6 \
    libxcursor1 \
    && rm -rf /var/lib/apt/lists/*
