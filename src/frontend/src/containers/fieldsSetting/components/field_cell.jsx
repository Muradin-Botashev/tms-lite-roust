import React, { useState } from 'react';
import { Button, Popup } from 'semantic-ui-react';

const FieldCell = ({ field, t, changeSettings, isExt, isDisabled, fieldName }) => {
    let [open, setOpen] = useState(false);

    const toggle = () => setOpen(prevState => !prevState);

    return (
        <>
            <div className="cell-field-name">{t(fieldName)}</div>
            <Popup
                trigger={
                    <Button
                        size="mini"
                        disabled={isDisabled}
                        className="margin-left-8"
                        onClick={toggle}
                    >
                        {t('All')}
                    </Button>
                }
                content={
                    <Button.Group>
                        {/*<Button
                            size="mini"
                            onClick={() => {
                                changeSettings(field, 'hidden', null, isExt);
                            }}
                        >
                            {t('hidden')}
                        </Button>*/}
                        <Button
                            size="mini"
                            onClick={() => {
                                changeSettings(field, 'show', null, isExt);
                                toggle();
                            }}
                        >
                            {t('show')}
                        </Button>
                        <Button
                            size="mini"
                            onClick={() => {
                                changeSettings(field, 'edit', null, isExt);
                                toggle();
                            }}
                        >
                            {t('edit')}
                        </Button>
                    </Button.Group>
                }
                open={open}
                on="click"
                hideOnScroll
                position="top left"
            />
        </>
    );
};

export default React.memo(FieldCell);
