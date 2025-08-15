@echo off
chcp 65001 >nul
echo ========================================
echo    UpdateBuilder - Debug Build Script
echo ========================================
echo.

:: Navegar para o diretório do projeto
cd /d "%~dp0.."

:: Verificar se o .NET 6.0 SDK está instalado
echo [1/4] Verificando .NET 6.0 SDK...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERRO: .NET SDK não encontrado!
    echo Por favor, instale o .NET 6.0 SDK
    echo Download: https://dotnet.microsoft.com/download/dotnet/6.0
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo .NET SDK encontrado: %DOTNET_VERSION%
echo.

:: Restaurar dependências
echo [2/4] Restaurando dependências NuGet...
dotnet restore
if %errorlevel% neq 0 (
    echo ERRO: Falha ao restaurar dependências!
    pause
    exit /b 1
)
echo Dependências restauradas com sucesso.
echo.

:: Compilar o projeto em modo Debug
echo [3/4] Compilando o projeto (Debug)...
dotnet build --configuration Debug --no-restore
if %errorlevel% neq 0 (
    echo ERRO: Falha na compilação!
    pause
    exit /b 1
)
echo Compilação Debug concluída com sucesso!
echo.

:: Executar o aplicativo
echo [4/4] Executando aplicativo...
if exist "bin\Debug\net6.0-windows\L2JCore Builder.exe" (
    echo Executável encontrado: bin\Debug\net6.0-windows\L2JCore Builder.exe
    echo.
    echo ========================================
    echo    EXECUTANDO UPDATEBUILDER (DEBUG)
    echo ========================================
    echo.
    dotnet run --configuration Debug
) else (
    echo ERRO: Executável não encontrado!
    echo Verifique se a compilação foi bem-sucedida.
    pause
    exit /b 1
)

echo.
echo Pressione qualquer tecla para sair...
pause >nul
