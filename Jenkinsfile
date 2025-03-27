pipeline {
    agent any  // Запускаем на любом доступном агенте

    environment {
        DEPLOY_SERVER = 'user@your-server'  // Данные сервера
        DEPLOY_PATH = '/path/to/your/app'  // Куда будем заливать проект
        SERVICE_NAME = 'your-app.service'  // Название systemd-сервиса
    }

    stages {
        stage('Checkout') {  // Клонирование репозитория
            steps {
                git branch: 'main', credentialsId: 'git-credentials-id', url: 'git@github.com:your-repo.git'
            }
        }
        
        stage('Build') {  // Сборка проекта
            steps {
                sh 'dotnet publish -c Release -o ./publish'
            }
        }
        
        stage('Deploy') {  // Деплой на сервер
            steps {
                sh '''
                rsync -avz --delete ./publish/ $DEPLOY_SERVER:$DEPLOY_PATH
                ssh $DEPLOY_SERVER "sudo systemctl restart $SERVICE_NAME"
                '''
            }
        }
    }
}
