import React, {useEffect} from 'react';
import {useDispatch, useSelector} from 'react-redux';
import {useTranslation} from 'react-i18next';
import DocWithEditor from '../../../../components/Documents/DocWithEditor';
import {
    clearDocuments,
    documentsSelector,
    getDocumentsRequest,
    progressSelector,
} from '../../../../ducks/documents';
import {Dimmer, Loader} from 'semantic-ui-react';

const Documents = ({gridName, cardId, isEditPermissions, load}) => {
    const {t} = useTranslation();
    const dispatch = useDispatch();

    const getDocuments = () => {
        dispatch(getDocumentsRequest({gridName, cardId}));
    };

    useEffect(() => {
        getDocuments();
        return () => {
            dispatch(clearDocuments());
        };
    }, []);

    const documents = useSelector(state => documentsSelector(state));
    const loading = useSelector(state => progressSelector(state));

    return (
        <div className="flex-container tabs-card">
          {/*  <Dimmer active={loading} inverted>
                <Loader size="huge">Loading</Loader>
            </Dimmer>*/}
            {documents.map((document, index) => (
                <DocWithEditor
                    key={document.fileId}
                    gridName={gridName}
                    cardId={cardId}
                    load={load}
                    document={document}
                    isEditPermissions={isEditPermissions}
                    getDocuments={getDocuments}
                    titleText={t('Edit document')}
                    okButtonText={t('SaveButton')}
                />
            ))}
            <DocWithEditor
                gridName={gridName}
                cardId={cardId}
                load={load}
                isEditPermissions={isEditPermissions}
                getDocuments={getDocuments}
                titleText={t('Add document')}
                okButtonText={t('AddButton')}
            />
        </div>
    );
};

export default Documents;
